import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, vi } from "vitest";

import { ChatDa } from "..";
import { useChatContext } from "../context/chat-context";
import { useChat } from "../hooks";
import { ChatProvider } from "../providers/chat-provider";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (k: string) => k }),
}));

vi.mock("@react-nano/use-event-source", () => ({
  useEventSource: vi.fn(() => [null]),
  useEventSourceListener: vi.fn(),
}));

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

const renderWithProviders = (ui: React.ReactNode) => {
  const client = createTestQueryClient();
  return render(
    <QueryClientProvider client={client}>
      <ChatProvider>{ui}</ChatProvider>
    </QueryClientProvider>,
  );
};

describe("<ChatDa />", () => {
  it("matches snapshot", () => {
    const { asFragment } = render(<ChatDa />);
    expect(asFragment()).toMatchSnapshot();
  });

  it("renders sidebar by default and toggles to fullscreen", () => {
    render(<ChatDa />);

    const toggleBtn = screen.getByRole("button", {
      name: "pages.chatDa.toggleView",
    });

    expect(toggleBtn).toBeInTheDocument();
    expect(screen.getByRole("complementary")).toBeInTheDocument();
    fireEvent.click(toggleBtn);
    expect(screen.queryByRole("complementary")).not.toBeInTheDocument();
  });
});

describe("useChat flow (no SSE streaming)", () => {
  const Harness = () => {
    const { sendMessage } = useChat();
    const { state } = useChatContext();

    const userCount = state.messages.filter((m) => m.sender === "user").length;
    const botCount = state.messages.filter((m) => m.sender === "bot").length;
    const botTyping = state.messages.some(
      (m) => m.sender === "bot" && !m.isComplete,
    );
    const last = state.messages[state.messages.length - 1];

    return (
      <div>
        <button onClick={() => sendMessage("hello")} aria-label="send">
          send
        </button>
        <div data-testid="count">{state.messages.length}</div>
        <div data-testid="userCount">{userCount}</div>
        <div data-testid="botCount">{botCount}</div>
        <div data-testid="botTyping">{botTyping ? "yes" : "no"}</div>
        <div data-testid="lastSender">{last?.sender ?? ""}</div>
        <div data-testid="firstText">{state.messages[0]?.text ?? ""}</div>
      </div>
    );
  };

  it("adds a user message and a bot placeholder; bot is “typing” (incomplete)", () => {
    renderWithProviders(<Harness />);

    expect(screen.getByTestId("count")).toHaveTextContent("0");
    fireEvent.click(screen.getByLabelText("send"));

    expect(screen.getByTestId("count")).toHaveTextContent("2");
    expect(screen.getByTestId("userCount")).toHaveTextContent("1");
    expect(screen.getByTestId("botCount")).toHaveTextContent("1");
    expect(screen.getByTestId("firstText")).toHaveTextContent("hello");
    expect(screen.getByTestId("botTyping")).toHaveTextContent("yes");
    expect(screen.getByTestId("lastSender")).toHaveTextContent("bot");
  });
});
