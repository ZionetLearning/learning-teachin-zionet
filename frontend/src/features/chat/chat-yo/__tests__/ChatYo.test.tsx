import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { ChatYo } from "../index";

// Mocking the hook used in ChatYo
vi.mock("../hooks", () => {
  return {
    useChat: () => ({
      sendMessage: vi.fn(),
      loading: false,
      messages: [
        {
          text: "Hello",
          position: "right",
          date: new Date(),
        },
        {
          text: "Hi there!",
          position: "left",
          date: new Date(),
        },
      ],
    }),
  };
});

// Snapshot and basic interaction test
describe("ChatYo", () => {
  it("renders chat messages correctly", () => {
    render(<ChatYo />);
    expect(screen.getByText("Hello")).toBeInTheDocument();
    expect(screen.getByText("Hi there!")).toBeInTheDocument();
  });

  it("allows typing and sending a message", () => {
    render(<ChatYo />);
    const input = screen.getByPlaceholderText("Type a message...");
    fireEvent.change(input, { target: { value: "Test message" } });
    expect(input).toHaveValue("Test message");

    const button = screen.getByRole("button", { name: "↑" });
    fireEvent.click(button);
    expect(input).toHaveValue(""); // input cleared after sending
  });

  it("matches snapshot", () => {
    const { asFragment } = render(<ChatYo />);
    expect(asFragment()).toMatchSnapshot();
  });
});
