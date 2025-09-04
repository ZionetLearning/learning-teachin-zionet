import React from "react";
import { render, screen, fireEvent } from "@testing-library/react";
import { vi, beforeEach } from "vitest";

// Mock react-i18next
let currentDir: "ltr" | "rtl" = "ltr";
vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (k: string) => k,
    i18n: { dir: () => currentDir },
  }),
}));

vi.mock("@mui/material", () => ({
  Box: ({ children, sx, ...props }: any) => (
    <div data-testid="mui-box" {...props}>
      {children}
    </div>
  ),
  Typography: ({ children, variant, ...props }: any) => (
    <div data-testid={`typography-${variant}`} {...props}>
      {children}
    </div>
  ),
  TextField: ({ value, onChange, disabled, ...props }: any) => (
    <input
      type="text"
      value={value}
      onChange={onChange}
      disabled={disabled}
      {...props}
    />
  ),
  Stack: ({ children, ...props }: any) => (
    <div data-testid="mui-stack" {...props}>
      {children}
    </div>
  ),
  Button: ({ children, onClick, disabled, ...props }: any) => (
    <button onClick={onClick} disabled={disabled} {...props}>
      {children}
    </button>
  ),
}));

// Mock the Button component (assuming it's a custom component)
vi.mock("../Button", () => ({
  Button: ({ children, onClick, disabled, variant, ...props }: any) => (
    <button
      onClick={onClick}
      disabled={disabled}
      data-variant={variant}
      {...props}
    >
      {children}
    </button>
  ),
}));

// Import the component after mocking
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

    // Titles/labels are translation keys (mocked)
    expect(screen.getByText("pages.profile.title")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.subTitle")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.secondSubTitle")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.firstName")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.lastName")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.email")).toBeInTheDocument();
    expect(screen.getByText("pages.profile.emailCannotBeChanged")).toBeInTheDocument();

    // Text fields show initial values
    const textboxes = screen.getAllByRole("textbox") as HTMLInputElement[];
    // Order: firstName, lastName, email
    expect(textboxes[0]).toHaveValue("Alice");
    expect(textboxes[1]).toHaveValue("Smith");
    expect(textboxes[2]).toHaveValue("alice@example.com");

    // Initially not editing => all inputs are disabled in your component
    textboxes.forEach((tb) => expect(tb).toBeDisabled());

    // Only the "Edit" button should be visible
    expect(screen.getByText("pages.profile.edit")).toBeInTheDocument();
    expect(screen.queryByText("pages.profile.saveChanges")).not.toBeInTheDocument();
    expect(screen.queryByText("pages.profile.cancel")).not.toBeInTheDocument();
  });

  it("enters edit mode and enables name fields; save disabled until dirty", () => {
    render(<Profile {...baseProps} />);

    // Enter edit mode
    fireEvent.click(screen.getByText("pages.profile.edit"));

    // Now Save/Cancel are visible
    const saveBtn = screen.getByText("pages.profile.saveChanges");
    const cancelBtn = screen.getByText("pages.profile.cancel");
    expect(saveBtn).toBeInTheDocument();
    expect(cancelBtn).toBeInTheDocument();

    // First/Last should be enabled; email stays disabled
    const [firstNameInput, lastNameInput, emailInput] = screen.getAllByRole("textbox") as HTMLInputElement[];
    expect(firstNameInput).toBeEnabled();
    expect(lastNameInput).toBeEnabled();
    expect(emailInput).toBeDisabled();

    // Not dirty yet -> save disabled
    expect(saveBtn).toBeDisabled();

    // Change first name -> becomes dirty -> save enabled
    fireEvent.change(firstNameInput, { target: { value: "Alicia" } });
    expect(saveBtn).toBeEnabled();
  });

  it("saves edited names and calls onSave with new values; exits edit mode", () => {
    const onSave = vi.fn();
    render(<Profile {...baseProps} onSave={onSave} />);

    fireEvent.click(screen.getByText("pages.profile.edit"));

    const [firstNameInput] = screen.getAllByRole("textbox") as HTMLInputElement[];
    fireEvent.change(firstNameInput, { target: { value: "Alicia" } });

    fireEvent.click(screen.getByText("pages.profile.saveChanges"));

    expect(onSave).toHaveBeenCalledTimes(1);
    expect(onSave).toHaveBeenCalledWith({ firstName: "Alicia", lastName: "Smith" });

    // Exited edit mode
    expect(screen.getByText("pages.profile.edit")).toBeInTheDocument();
    expect(screen.queryByText("pages.profile.saveChanges")).not.toBeInTheDocument();
  });

  it("cancels edits (resets values) and exits edit mode without calling onSave", () => {
    const onSave = vi.fn();
    render(<Profile {...baseProps} onSave={onSave} />);

    fireEvent.click(screen.getByText("pages.profile.edit"));

    const [, lastNameInput] = screen.getAllByRole("textbox") as HTMLInputElement[];
    fireEvent.change(lastNameInput, { target: { value: "Smyth" } });

    fireEvent.click(screen.getByText("pages.profile.cancel"));

    // Back to non-editing state
    expect(screen.getByText("pages.profile.edit")).toBeInTheDocument();

    // Values reset to props
    fireEvent.click(screen.getByText("pages.profile.edit")); // re-open to inspect inputs
    const [firstNameInput2, lastNameInput2] = screen.getAllByRole("textbox") as HTMLInputElement[];
    expect(firstNameInput2).toHaveValue("Alice");
    expect(lastNameInput2).toHaveValue("Smith");

    expect(onSave).not.toHaveBeenCalled();
  });

  it("updates inputs when parent props change", () => {
    const { rerender } = render(<Profile {...baseProps} />);

    // Change props externally (simulate parent update)
    rerender(<Profile {...baseProps} firstName="Alicia" lastName="Smyth" />);

    const [firstNameInput, lastNameInput] = screen.getAllByRole("textbox") as HTMLInputElement[];
    expect(firstNameInput).toHaveValue("Alicia");
    expect(lastNameInput).toHaveValue("Smyth");
  });

  it("save button stays disabled if nothing changed", () => {
    render(<Profile {...baseProps} />);

    fireEvent.click(screen.getByText("pages.profile.edit"));
    const saveBtn = screen.getByText("pages.profile.saveChanges");
    expect(saveBtn).toBeDisabled();

    // Type the same value with extra spaces -> trim makes it still not dirty if equal after trim
    const [firstNameInput] = screen.getAllByRole("textbox") as HTMLInputElement[];
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