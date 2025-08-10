import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";
import { ChatSh } from "../";

// mocking the service call
const sendChatMessageMock = vi.fn();
vi.mock("../services", () => ({
  sendChatMessage: (...args: unknown[]) => sendChatMessageMock(...args),
}));

describe("<ChatSh />", () => {
  beforeEach(() => {
    sendChatMessageMock.mockReset();
  });

  it("sends a message, streams AI reply, and re-enables the button", async () => {
    const aiReply = "Hello from AI";
    sendChatMessageMock.mockResolvedValueOnce(aiReply);

    render(<ChatSh />);

    const user = userEvent.setup();
    const input = screen.getByRole("textbox");
    const sendBtn = screen.getByRole("button");

    // type + click
    await user.type(input, "Hi there");
    await user.click(sendBtn);

    // user msg bubble appears
    expect(await screen.findByText("Hi there")).toBeInTheDocument();

    // ensure service was called before waiting for stream
    await waitFor(() => expect(sendChatMessageMock).toHaveBeenCalledTimes(1));

    expect(await screen.findByText(aiReply)).toBeInTheDocument();

    // button returns to enabled + arrow
    await waitFor(() => {
      expect(sendBtn).not.toBeDisabled();
      expect(sendBtn).toHaveTextContent("➤");
    });
  });

  it("sends on Enter key", async () => {
    sendChatMessageMock.mockResolvedValueOnce("OK");

    render(<ChatSh />);
    const user = userEvent.setup();

    const input = screen.getByRole("textbox");
    await user.type(input, "Ping{enter}");

    // user msg bubble
    expect(await screen.findByText("Ping")).toBeInTheDocument();

    // assistant (the ai) bubble after streaming
    expect(await screen.findByText("OK")).toBeInTheDocument();
  });

  it("alerts on error and stops loading", async () => {
    const alertSpy = vi.spyOn(window, "alert").mockImplementation(() => {});
    sendChatMessageMock.mockRejectedValueOnce(new Error("boom"));

    render(<ChatSh />);
    const user = userEvent.setup();

    const input = screen.getByRole("textbox");
    const sendBtn = screen.getByRole("button");

    await user.type(input, "Fail please");
    await user.click(sendBtn);

    // allow the rejected promise to settle and UI to update
    await waitFor(() => {
      expect(alertSpy).toHaveBeenCalled();
      expect(sendBtn).not.toBeDisabled();
      expect(sendBtn).toHaveTextContent("➤");
    });

    alertSpy.mockRestore();
  });
});
