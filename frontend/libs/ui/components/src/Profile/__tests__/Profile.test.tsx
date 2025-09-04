import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { vi, beforeEach } from "vitest";

// --- i18n mock (typed) ---
let currentDir: "ltr" | "rtl" = "ltr";
vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (k: string) => k,
    i18n: { dir: () => currentDir },
  }),
}));

// --- Types from react (static type import) ---
import type {
  PropsWithChildren,
  HTMLAttributes,
  InputHTMLAttributes,
  ButtonHTMLAttributes,
} from "react";

// --- @mui/material mock (no `any`, no MUI props forwarded) ---
vi.mock("@mui/material", () => {
  type DivProps = HTMLAttributes<HTMLDivElement>;
  type InputProps = InputHTMLAttributes<HTMLInputElement>;
  type BtnProps = ButtonHTMLAttributes<HTMLButtonElement>;

  const Box = ({ children }: PropsWithChildren<{}>) => (
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
  }) => <input type="text" value={value} onChange={onChange} disabled={disabled} />;

  const Stack = ({ children }: PropsWithChildren<{}>) => (
    <div data-testid="mui-stack">{children}</div>
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

  return { Box, Typography, TextField, Stack, Button };
});

// --- Your custom Button mock (typed) ---
vi.mock("../Button", () => {
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

  return { Button };
});

// ---- Under test (import after mocks) ----
import { Profile } from "../index";

const baseProps = {
  firstName: "Alice",
  lastName: "Smith",
  email: "alice@example.com",
};

beforeEach(() => {
  vi.clearAllMocks();
  currentDir = "ltr";
});

describe("<Profile />", () => {
  it("renders titles/labels and initial values", () => {
    render(<Profile {...baseProps} />);

    expect(screen.getByText("pages.profile.title")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.subTitle")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.secondSubTitle")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.firstName")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.lastName")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.email")).toBeInTheDocument();
    expect(
      screen.getByText("pages.profile.emailCannotBeChanged")
    ).toBeInTheDocument();

    const textboxes = screen.getAllByRole("textbox") as HTMLInputElement[];
    expect(textboxes[0]).toHaveValue("Alice");
    expect(textboxes[1]).toHaveValue("Smith");
    expect(textboxes[2]).toHaveValue("alice@example.com");

    textboxes.forEach((tb) => expect(tb).toBeDisabled());

    expect(screen.getByText("pages.profile.edit")).toBeInTheDocument();
    expect(screen.queryByText("pages.profile.saveChanges")).not.toBeInTheDocument();
    expect(screen.queryByText("pages.profile.cancel")).not.toBeInTheDocument();
  });

  it("enters edit mode and enables name fields; save disabled until dirty", () => {
    render(<Profile {...baseProps} />);
    fireEvent.click(screen.getByText("pages.profile.edit"));

    const saveBtn = screen.getByText("pages.profile.saveChanges");
    const cancelBtn = screen.getByText("pages.profile.cancel");
    expect(saveBtn).toBeInTheDocument();
    expect(cancelBtn).toBeInTheDocument();

    const [firstNameInput, lastNameInput, emailInput] =
      screen.getAllByRole("textbox") as HTMLInputElement[];
    expect(firstNameInput).toBeEnabled();
    expect(lastNameInput).toBeEnabled();
    expect(emailInput).toBeDisabled();

    expect(saveBtn).toBeDisabled();

    fireEvent.change(firstNameInput, { target: { value: "Alicia" } });
    expect(saveBtn).toBeEnabled();
  });

  it("saves edited names and calls onSave; exits edit mode", () => {
    const onSave = vi.fn();
    render(<Profile {...baseProps} onSave={onSave} />);

    fireEvent.click(screen.getByText("pages.profile.edit"));

    const [firstNameInput] = screen.getAllByRole("textbox") as HTMLInputElement[];
    fireEvent.change(firstNameInput, { target: { value: "Alicia" } });

    fireEvent.click(screen.getByText("pages.profile.saveChanges"));
    expect(onSave).toHaveBeenCalledTimes(1);
    expect(onSave).toHaveBeenCalledWith({ firstName: "Alicia", lastName: "Smith" });

    expect(screen.getByText("pages.profile.edit")).toBeInTheDocument();
    expect(screen.queryByText("pages.profile.saveChanges")).not.toBeInTheDocument();
  });

  it("cancels edits and resets values", () => {
    const onSave = vi.fn();
    render(<Profile {...baseProps} onSave={onSave} />);

    fireEvent.click(screen.getByText("pages.profile.edit"));

    const [, lastNameInput] = screen.getAllByRole("textbox") as HTMLInputElement[];
    fireEvent.change(lastNameInput, { target: { value: "Smyth" } });

    fireEvent.click(screen.getByText("pages.profile.cancel"));

    expect(screen.getByText("pages.profile.edit")).toBeInTheDocument();

    fireEvent.click(screen.getByText("pages.profile.edit"));
    const [firstNameInput2, lastNameInput2] =
      screen.getAllByRole("textbox") as HTMLInputElement[];
    expect(firstNameInput2).toHaveValue("Alice");
    expect(lastNameInput2).toHaveValue("Smith");

    expect(onSave).not.toHaveBeenCalled();
  });

  it("updates inputs when parent props change", () => {
    const { rerender } = render(<Profile {...baseProps} />);
    rerender(<Profile {...baseProps} firstName="Alicia" lastName="Smyth" />);

    const [firstNameInput, lastNameInput] =
      screen.getAllByRole("textbox") as HTMLInputElement[];
    expect(firstNameInput).toHaveValue("Alicia");
    expect(lastNameInput).toHaveValue("Smyth");
  });

  it("save button stays disabled if nothing changed", () => {
    render(<Profile {...baseProps} />);

    fireEvent.click(screen.getByText("pages.profile.edit"));
    const saveBtn = screen.getByText("pages.profile.saveChanges");
    expect(saveBtn).toBeDisabled();

    const [firstNameInput] =
      screen.getAllByRole("textbox") as HTMLInputElement[];
    fireEvent.change(firstNameInput, { target: { value: "  Alice  " } });
    expect(saveBtn).toBeDisabled();
  });

  it("renders correctly and matches snapshot", () => {
    const { asFragment } = render(
      <Profile
        firstName="John"
        lastName="Doe"
        email="john.doe@example.com"
        onSave={vi.fn()}
      />
    );
    expect(asFragment()).toMatchSnapshot();
  });
});
