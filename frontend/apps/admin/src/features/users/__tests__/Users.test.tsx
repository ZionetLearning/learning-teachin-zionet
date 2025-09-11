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

vi.mock("@admin/api", () => ({
  useGetAllUsers: vi.fn(),
  useDeleteUserByUserId: vi.fn(),
}));

vi.mock("@app-providers/api/user", () => ({
  useCreateUser: vi.fn(),
  useUpdateUserByUserId: vi.fn(),
}));

import { useGetAllUsers, useDeleteUserByUserId } from "@admin/api";
import { useCreateUser, useUpdateUserByUserId } from "@app-providers/api/user";

// Type the mocked functions properly
const mockUseGetAllUsers = useGetAllUsers as ReturnType<typeof vi.fn>;
const mockUseCreateUser = useCreateUser as ReturnType<typeof vi.fn>;
const mockUseUpdateUserByUserId = useUpdateUserByUserId as ReturnType<typeof vi.fn>;
const mockUseDeleteUserByUserId = useDeleteUserByUserId as ReturnType<typeof vi.fn>;

const updateMutate = vi.fn();
const deleteMutate = vi.fn();

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

  // Mock hooks using the properly typed variables
  mockUseGetAllUsers.mockReturnValue(rq({ data: sampleUsers }));
  mockUseCreateUser.mockReturnValue({ mutate: vi.fn(), isPending: false });
  mockUseUpdateUserByUserId.mockImplementation(() => ({
    mutate: updateMutate,
    isPending: false,
  }));
  mockUseDeleteUserByUserId.mockImplementation(() => ({
    mutate: deleteMutate,
    isPending: false,
  }));
});

describe("<Users />", () => {
  it("matches snapshot (with users)", () => {
    const { asFragment } = renderUsers();
    expect(asFragment()).toMatchSnapshot();
  });

  it("renders loading state", () => {
    mockUseGetAllUsers.mockReturnValue(rq({ isLoading: true }));
    renderUsers();
    expect(screen.getByText("pages.users.loadingUsers")).toBeInTheDocument();
  });

  it("renders error state", () => {
    mockUseGetAllUsers.mockReturnValue(
      rq({ error: new Error("Boom"), data: undefined }),
    );
    renderUsers();
    expect(screen.getByText("pages.users.userNotFound")).toBeInTheDocument();
  });

  it("submits create user form", async () => {
    const mutate = vi.fn((userData: {
      userId: string;
      email: string;
      password: string;
      firstName: string;
      lastName: string;
      role: string;
    }, opts?: {
      onSuccess?: () => void;
      onError?: (error: Error) => void;
      onSettled?: () => void;
    }) => {
      opts?.onSuccess?.();
      opts?.onSettled?.();
    });

    mockUseCreateUser.mockReturnValue({ mutate, isPending: false });

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
    expect(arg.userId).toMatch(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i);
  });

  it("updates a user via inline edit form (partial fields only)", async () => {
    mockUseGetAllUsers.mockReturnValue(rq({ data: [sampleUsers[0]] }));
    renderUsers();

    fireEvent.click(screen.getByTestId("users-update-btn"));
    fireEvent.change(screen.getByTestId("users-edit-email"), {
      target: { value: "changed@example.com" },
    });
    fireEvent.change(screen.getByTestId("users-edit-first-name"), {
      target: { value: "Charlie" },
    });
    fireEvent.click(screen.getByTestId("users-edit-save"));

    await waitFor(() => expect(updateMutate).toHaveBeenCalledTimes(1));
    expect(updateMutate.mock.calls[0][0]).toEqual({
      email: "changed@example.com",
      firstName: "Charlie",
    });
  });

  it("deletes a user", () => {
    mockUseGetAllUsers.mockReturnValue(rq({ data: [sampleUsers[0]] }));
    const confirmSpy = vi.spyOn(window, "confirm").mockReturnValue(true);

    renderUsers();
    fireEvent.click(screen.getByRole("button", { name: /delete/i }));

    expect(confirmSpy).toHaveBeenCalled();
    expect(deleteMutate).toHaveBeenCalledTimes(1);
    confirmSpy.mockRestore();
  });
});