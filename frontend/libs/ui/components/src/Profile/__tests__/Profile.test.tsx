import { render, screen, fireEvent } from "@testing-library/react";
import { vi, beforeEach, describe, it, expect } from "vitest";

// --- i18n mock ---
let currentDir: "ltr" | "rtl" = "ltr";
vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (k: string) => k,
    i18n: { dir: () => currentDir },
  }),
}));

import type {
  PropsWithChildren,
  InputHTMLAttributes,
  ButtonHTMLAttributes,
} from "react";

// --- @mui/material mock ---
vi.mock("@mui/material", () => {
  type InputProps = InputHTMLAttributes<HTMLInputElement>;
  type BtnProps = ButtonHTMLAttributes<HTMLButtonElement>;
  type WithChildren = PropsWithChildren<unknown>;

  const Box = ({ children }: WithChildren) => (
    <div data-testid="mui-box">{children}</div>
  );

  const Typography = ({
    children,
    variant,
  }: PropsWithChildren<{ variant?: string }>) => (
    <div data-testid={`typography-${variant ?? "body"}`}>{children}</div>
  );

  const TextField = ({
    value,
    onChange,
    disabled,
  }: {
    value?: InputProps["value"];
    onChange?: InputProps["onChange"];
    disabled?: boolean;
  }) => (
    <input type="text" value={value} onChange={onChange} disabled={disabled} />
  );

  const Stack = ({ children }: WithChildren) => (
    <div data-testid="mui-stack">{children}</div>
  );

  const Grid = ({ children }: WithChildren & Record<string, unknown>) => (
    <div data-testid="mui-grid">{children}</div>
  );

  const Button = ({
    children,
    onClick,
    disabled,
  }: PropsWithChildren<Pick<BtnProps, "onClick" | "disabled">>) => (
    <button onClick={onClick} disabled={disabled}>
      {children}
    </button>
  );

  return { Box, Typography, TextField, Stack, Button, Grid };
});

vi.mock("@ui-components", () => {
  type CustomBtnProps = ButtonHTMLAttributes<HTMLButtonElement> & {
    variant?: string;
  };

  const Button = ({
    children,
    onClick,
    disabled,
    variant,
  }: PropsWithChildren<CustomBtnProps>) => (
    <button onClick={onClick} disabled={disabled} data-variant={variant}>
      {children}
    </button>
  );

  const Dropdown = ({
    value,
    onChange,
    options,
  }: {
    value?: string;
    onChange?: (v: string) => void;
    options?: { value: string; label: string }[];
  }) => (
    <select
      data-testid="ui-dropdown"
      value={value}
      onChange={(e) => onChange?.(e.target.value)}
    >
      {(options ?? []).map((o) => (
        <option key={o.value} value={o.value}>
          {o.label}
        </option>
      ))}
    </select>
  );

  const InterestChip = ({
    label,
    onDelete,
  }: {
    label: string;
    onDelete?: () => void;
  }) => (
    <span data-testid="interest-chip">
      {label}
      {onDelete && (
        <button aria-label={`delete-${label}`} onClick={onDelete}>
          x
        </button>
      )}
    </span>
  );

  return { Button, Dropdown, InterestChip };
});

// --- @app-providers mock: provide the mutation hook + types used in the file ---
const mutateAsyncMock = vi.fn();
vi.mock("@app-providers", () => {
  return {
    // minimal shape to satisfy the component’s imports
    useUpdateUserByUserId: vi.fn(() => ({
      mutateAsync: mutateAsyncMock,
    })),
    toAppRole: (role: unknown) => role,
  };
});

// ---- Under test (import after mocks) ----
import { Profile } from "../index";

// Helper user
const user = {
  userId: "user-123",
  firstName: "Alice",
  lastName: "Smith",
  email: "alice@example.com",
  role: "student" as const,
};

beforeEach(() => {
  vi.clearAllMocks();
  currentDir = "ltr";
});

describe("<Profile />", () => {
  it("renders titles/labels and initial values; name inputs enabled, email disabled; save/cancel disabled initially", () => {
    render(<Profile user={user} />);

    // labels/titles
    expect(screen.getByText("pages.profile.title")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.subTitle")).toBeInTheDocument();
    expect(
      screen.getByText("pages.profile.secondSubTitle"),
    ).toBeInTheDocument();
    expect(screen.getByText("pages.profile.firstName")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.lastName")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.email")).toBeInTheDocument();
    expect(
      screen.getByText("pages.profile.emailCannotBeChanged"),
    ).toBeInTheDocument();

    const inputs = screen.getAllByRole("textbox") as HTMLInputElement[];
    // There are 4 textboxes for student: first, last, interests, and email
    expect(inputs.length).toBeGreaterThanOrEqual(3);

    const firstNameInput = inputs[0];
    const lastNameInput = inputs[1];
    const emailInput = inputs[inputs.length - 1];
    expect(firstNameInput).toHaveValue("Alice");
    expect(lastNameInput).toHaveValue("Smith");
    expect(emailInput).toHaveValue("alice@example.com");

    expect(firstNameInput).toBeEnabled();
    expect(lastNameInput).toBeEnabled();
    expect(emailInput).toBeDisabled();

    const saveBtn = screen.getByText("pages.profile.saveChanges");
    const cancelBtn = screen.getByText("pages.profile.cancel");
    expect(saveBtn).toBeDisabled();
    expect(cancelBtn).toBeDisabled();
  });

  it("enables Save/Cancel when dirty; trimming to same value keeps them disabled", () => {
    render(<Profile user={user} />);

    const saveBtn = screen.getByText(
      "pages.profile.saveChanges",
    ) as HTMLButtonElement;
    const cancelBtn = screen.getByText(
      "pages.profile.cancel",
    ) as HTMLButtonElement;
    const [firstNameInput] = screen.getAllByRole(
      "textbox",
    ) as HTMLInputElement[];

    // Not dirty initially
    expect(saveBtn).toBeDisabled();
    expect(cancelBtn).toBeDisabled();

    // Change to a different value -> becomes dirty
    fireEvent.change(firstNameInput, { target: { value: "Alicia" } });
    expect(saveBtn).toBeEnabled();
    expect(cancelBtn).toBeEnabled();

    // Change to a trimmed-equal value -> back to original -> not dirty
    fireEvent.change(firstNameInput, { target: { value: "  Alice  " } });
    expect(saveBtn).toBeDisabled();
    expect(cancelBtn).toBeDisabled();
  });

  it("Cancel reverts edits and disables buttons again", () => {
    render(<Profile user={user} />);

    const saveBtn = screen.getByText("pages.profile.saveChanges");
    const cancelBtn = screen.getByText("pages.profile.cancel");
    const [firstNameInput] = screen.getAllByRole(
      "textbox",
    ) as HTMLInputElement[];

    fireEvent.change(firstNameInput, { target: { value: "Alicia" } });
    expect(saveBtn).toBeEnabled();
    expect(cancelBtn).toBeEnabled();

    fireEvent.click(cancelBtn);
    expect(firstNameInput).toHaveValue("Alice");
    expect(saveBtn).toBeDisabled();
    expect(cancelBtn).toBeDisabled();
  });

  it("calls update mutation with trimmed values on Save", async () => {
    render(<Profile user={user} />);

    const saveBtn = screen.getByText("pages.profile.saveChanges");
    const [firstNameInput, lastNameInput] = screen.getAllByRole(
      "textbox",
    ) as HTMLInputElement[];

    fireEvent.change(firstNameInput, { target: { value: "  Alicia  " } });
    fireEvent.change(lastNameInput, { target: { value: "  Smyth  " } });

    // enable save now
    expect(saveBtn).toBeEnabled();

    // mock resolves
    mutateAsyncMock.mockResolvedValueOnce({ ok: true });

    // click save
    await fireEvent.click(saveBtn);

    expect(mutateAsyncMock).toHaveBeenCalledTimes(1);
    expect(mutateAsyncMock).toHaveBeenCalledWith(
      expect.objectContaining({
        email: "alice@example.com",
        firstName: "Alicia",
        lastName: "Smyth",
      }),
    );
  });

  it("updates inputs when parent user prop changes", () => {
    const { rerender } = render(<Profile user={user} />);

    rerender(
      <Profile
        user={{
          ...user,
          firstName: "Alicia",
          lastName: "Smyth",
        }}
      />,
    );

    const [firstNameInput, lastNameInput] = screen.getAllByRole(
      "textbox",
    ) as HTMLInputElement[];
    expect(firstNameInput).toHaveValue("Alicia");
    expect(lastNameInput).toHaveValue("Smyth");
  });

  it("matches snapshot", () => {
    const { asFragment } = render(<Profile user={user} />);
    expect(asFragment()).toMatchSnapshot();
  });
});
