import {
  QueryClient,
  QueryClientProvider,
  type UseQueryResult,
} from "@tanstack/react-query";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { Users } from "../index";
import { User } from "@app-providers";

vi.mock("react-i18next", async () => {
  const actual =
    await vi.importActual<typeof import("react-i18next")>("react-i18next");
  return {
    ...actual,
    useTranslation: () => ({
      t: (k: string) => k,
      i18n: { changeLanguage: vi.fn(), dir: () => "ltr" },
    }),
  };
});

vi.mock("react-toastify", () => ({
  toast: { success: vi.fn(), error: vi.fn() },
}));

vi.mock("@admin/api", async () => {
  const actual = await vi.importActual<typeof import("@admin/api")>("@admin/api");
  return {
    ...actual,
    useDeleteUserByUserId: vi.fn(),
    useUpdateUserByUserId: vi.fn(),
  };
});

vi.mock("@app-providers", async () => {
  const actual = (await vi.importActual("@app-providers")) as Record<
    string,
    unknown
  >;
  return {
    ...actual,
    useCreateUser: vi.fn(),
  };
});

const { useGetAllUsers, useUpdateUserByUserId, useDeleteUserByUserId } =
  vi.mocked(await import("@admin/api")) as unknown as {
    useGetAllUsers: ReturnType<typeof vi.fn>;
    useUpdateUserByUserId: ReturnType<typeof vi.fn>;
    useDeleteUserByUserId: ReturnType<typeof vi.fn>;
  };

const { useCreateUser } = vi.mocked(
  await import("@app-providers"),
) as unknown as {
  useCreateUser: ReturnType<typeof vi.fn>;
};

const updateMutate = vi.fn();

const rq = (
  over: Partial<UseQueryResult<User[], Error>>,
): UseQueryResult<User[], Error> =>
  ({
    data: undefined,
    error: null,
    isLoading: false,
    isError: !!over.error,
    isSuccess: !!over.data && !over.error,
    fetchStatus: "idle",
    status: over.isLoading ? "pending" : over.error ? "error" : "success",
    refetch: vi.fn(),
    failureCount: 0,
    isPending: over.isLoading ?? false,
    isFetched: true,
    isInitialLoading: false,
    isFetching: false,
    dataUpdatedAt: Date.now(),
    errorUpdatedAt: 0,
    isPaused: false,
    ...over,
  }) as UseQueryResult<User[], Error>;

const sampleUsers: User[] = [
  {
    userId: "u1",
    email: "alpha@example.com",
    firstName: "Alice",
    lastName: "Anderson",
  },
  {
    userId: "u2",
    email: "beta@example.com",
    firstName: "Bob",
    lastName: "Baker",
  },
];

let qc: QueryClient;
const renderUsers = () =>
  render(
    <QueryClientProvider client={qc}>
      <Users />
    </QueryClientProvider>,
  );

beforeEach(() => {
  vi.clearAllMocks();
  qc = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  if (globalThis.crypto && "randomUUID" in globalThis.crypto) {
    try {
      vi.spyOn(globalThis.crypto, "randomUUID").mockReturnValue(
        "123e4567-e89b-12d3-a456-426614174000",
      );
    } catch {
      const existing = globalThis.crypto as Crypto;
      Object.defineProperty(globalThis, "crypto", {
        value: {
          ...existing,
          randomUUID: () => "123e4567-e89b-12d3-a456-426614174000",
        },
        configurable: true,
      });
    }
  } else {
    Object.defineProperty(globalThis, "crypto", {
      value: { randomUUID: () => "123e4567-e89b-12d3-a456-426614174000" },
      configurable: true,
    });
  }
});

describe("<Users />", () => {
  it("matches snapshot (with users)", () => {
    useGetAllUsers.mockReturnValue(rq({ data: sampleUsers }));
    useCreateUser.mockReturnValue({ mutate: vi.fn(), isPending: false });
    (useUpdateUserByUserId).mockImplementation(() => ({
      mutate: updateMutate,
      isPending: false,
    }));

    const { asFragment } = renderUsers();
    expect(asFragment()).toMatchSnapshot();
  });

  it("renders loading state", () => {
    useGetAllUsers.mockReturnValue(rq({ isLoading: true }));
    useCreateUser.mockReturnValue({ mutate: vi.fn(), isPending: false });
    renderUsers();
    expect(screen.getByText("pages.users.loadingUsers")).toBeInTheDocument();
  });

  it("renders error state", () => {
    useGetAllUsers.mockReturnValue(
      rq({ error: new Error("Boom"), data: undefined }),
    );
    useCreateUser.mockReturnValue({ mutate: vi.fn(), isPending: false });
    renderUsers();
    expect(screen.getByText("pages.users.userNotFound")).toBeInTheDocument();
  });

  it("submits create user form", async () => {
    type Vars = {
      userId: string;
      email: string;
      password: string;
      firstName: string;
      lastName: string;
    };
    interface Handlers {
      onSuccess?: () => void;
      onError?: (e: Error) => void;
      onSettled?: () => void;
    }
    const mutate = vi.fn((_: Vars, opts?: Handlers) => {
      opts?.onSuccess?.();
      opts?.onSettled?.();
    });
    //const updateMutate = vi.fn();
    useGetAllUsers.mockReturnValue(rq({ data: sampleUsers }));
    useCreateUser.mockReturnValue({ mutate, isPending: false });
    useUpdateUserByUserId.mockImplementation(() => {
      return {
        mutate: updateMutate,
        isPending: false,
      };
    });
    useDeleteUserByUserId.mockImplementation(() => ({
      mutate: vi.fn(),
      isPending: false,
    }));

    renderUsers();
    fireEvent.change(screen.getByPlaceholderText(/user@example.com/i), {
      target: { value: "new@example.com" },
    });
    fireEvent.change(screen.getByPlaceholderText(/John/i), {
      target: { value: "NewFirst" },
    });
    fireEvent.change(screen.getByPlaceholderText(/Doe/i), {
      target: { value: "NewLast" },
    });
    fireEvent.change(screen.getByPlaceholderText(/\*\*\*\*\*\*/i), {
      target: { value: "Secret123!" },
    });
    fireEvent.click(screen.getByTestId("users-create-submit"));
    await waitFor(() => expect(mutate).toHaveBeenCalledTimes(1));
    const arg = mutate.mock.calls[0][0];
    expect(arg.email).toBe("new@example.com");
    expect(arg.password).toBe("Secret123!");
    expect(arg.firstName).toBe("NewFirst");
    expect(arg.lastName).toBe("NewLast");
    expect(arg.userId).toBe("123e4567-e89b-12d3-a456-426614174000");
  });

  it("updates a user via inline edit form (partial fields only)", async () => {
    const updateMutate = vi.fn();
    useGetAllUsers.mockReturnValue(rq({ data: [sampleUsers[0]] }));
    useCreateUser.mockReturnValue({ mutate: vi.fn(), isPending: false });
    useUpdateUserByUserId.mockImplementation(() => {
      return {
        mutate: updateMutate,
        isPending: false,
      };
    });
    useDeleteUserByUserId.mockImplementation(() => ({
      mutate: vi.fn(),
      isPending: false,
    }));

    renderUsers();

    // open the row editor (guaranteed button)
    fireEvent.click(screen.getByTestId("users-update-btn"));

    // edit the row inputs (guaranteed inputs)
    fireEvent.change(screen.getByTestId("users-edit-email"), {
      target: { value: "changed@example.com" },
    });
    fireEvent.change(screen.getByTestId("users-edit-first-name"), {
      target: { value: "Charlie" },
    });

    // submit the row editor form (guaranteed button)
    fireEvent.click(screen.getByTestId("users-edit-save"));

    // the mutate call is sync in your impl, but waitFor makes this robust
    await waitFor(() => expect(updateMutate).toHaveBeenCalledTimes(1));
    expect(updateMutate.mock.calls[0][0]).toEqual({
      userId: "u1",
      email: "changed@example.com",
      firstName: "Charlie",
    });
  });

  it("deletes a user", () => {
    const deleteMutate = vi.fn();
    //const updateMutate = vi.fn();
    useGetAllUsers.mockReturnValue(rq({ data: [sampleUsers[0]] }));
    useCreateUser.mockReturnValue({ mutate: vi.fn(), isPending: false });
    useUpdateUserByUserId.mockImplementation(() => {
      return {
        mutate: updateMutate,
        isPending: false,
      };
    });
    useDeleteUserByUserId.mockImplementation(() => ({
      mutate: deleteMutate,
      isPending: false,
    }));

    const confirmSpy = vi.spyOn(window, "confirm").mockReturnValue(true);
    renderUsers();
    fireEvent.click(screen.getByRole("button", { name: /delete/i }));
    expect(confirmSpy).toHaveBeenCalled();
    expect(deleteMutate).toHaveBeenCalledTimes(1);
    confirmSpy.mockRestore();
  });
});
