import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import "@testing-library/jest-dom/vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, it, expect, vi, beforeEach } from "vitest";
import "./__mocks__";
import { TypingPractice } from "..";
import { speakSpy } from "./__mocks__";

describe("<TypingPractice />", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderWithProviders = () => {
    const qc = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    return render(
      <QueryClientProvider client={qc}>
        <TypingPractice />
      </QueryClientProvider>,
    );
  };

  vi.mock("lottie-web", () => ({
    __esModule: true,
    default: {
      loadAnimation: vi.fn(),
      destroy: vi.fn(),
    },
  }));

  vi.mock("react-lottie", () => ({
    __esModule: true,
    default: vi.fn(() => <div data-testid="lottie-animation" />),
  }));

  it("shows welcome screen initially", () => {
    renderWithProviders();
    expect(screen.getByTestId("typing-welcome-screen")).toBeInTheDocument();
  });

  it("matches snapshot (initial welcome screen)", () => {
    const { asFragment } = renderWithProviders();
    expect(asFragment()).toMatchSnapshot();
  });

  it("shows level selection modal when configure is clicked", () => {
    renderWithProviders();
    fireEvent.click(screen.getByTestId("typing-configure-button"));
    expect(screen.getByTestId("typing-level-selection")).toBeInTheDocument();
  });

  it("selects a level and shows ready phase with play button", () => {
    renderWithProviders();
    fireEvent.click(screen.getByTestId("typing-configure-button"));
    fireEvent.click(screen.getByTestId("typing-level-easy"));
    fireEvent.click(screen.getByTestId("game-config-start"));
    expect(screen.getByTestId("typing-exercise-area")).toBeInTheDocument();
    expect(screen.getByTestId("typing-selected-level")).toHaveTextContent(
      "easy",
    );
    expect(screen.getByTestId("typing-play")).toBeInTheDocument();
  });

  it("plays audio then advances to typing phase and enables replay", async () => {
    renderWithProviders();
    fireEvent.click(screen.getByTestId("typing-configure-button"));
    fireEvent.click(screen.getByTestId("typing-level-easy"));
    fireEvent.click(screen.getByTestId("game-config-start"));
    fireEvent.click(screen.getByTestId("typing-play"));
    await screen.findByTestId("typing-phase-typing");
    expect(screen.getByTestId("typing-replay")).toBeInTheDocument();
    expect(speakSpy).toHaveBeenCalledWith("שלום");
  });

  it("submits an answer and shows accuracy feedback", async () => {
    renderWithProviders();
    fireEvent.click(screen.getByTestId("typing-configure-button"));
    fireEvent.click(screen.getByTestId("typing-level-easy"));
    fireEvent.click(screen.getByTestId("game-config-start"));
    fireEvent.click(screen.getByTestId("typing-play"));
    await screen.findByTestId("typing-phase-typing");
    fireEvent.change(screen.getByTestId("typing-input"), {
      target: { value: "שלום" },
    });
    fireEvent.click(screen.getByTestId("typing-submit"));
    
    // Just wait for any accuracy feedback to appear
    await waitFor(() => {
      const accuracyElement = screen.getByText(/% pages\.typingPractice\.accuracy/);
      expect(accuracyElement).toBeInTheDocument();
    });
  });

  it("shows try again functionality after submitting answer", async () => {
    renderWithProviders();
    fireEvent.click(screen.getByTestId("typing-configure-button"));
    fireEvent.click(screen.getByTestId("typing-level-easy"));
    fireEvent.click(screen.getByTestId("game-config-start"));
    fireEvent.click(screen.getByTestId("typing-play"));
    await screen.findByTestId("typing-phase-typing");
    fireEvent.change(screen.getByTestId("typing-input"), {
      target: { value: "test input" },
    });
    fireEvent.click(screen.getByTestId("typing-submit"));
    
    // Wait for feedback to appear
    await waitFor(() => {
      expect(screen.getByText("pages.typingPractice.tryAgain")).toBeInTheDocument();
    });
    
    // Click try again and verify input is cleared
    fireEvent.click(screen.getByText("pages.typingPractice.tryAgain"));
    await screen.findByTestId("typing-phase-typing");
    expect((screen.getByTestId("typing-input") as HTMLInputElement).value).toBe("");
  });

  it("completes exercise flow successfully", async () => {
    renderWithProviders();
    fireEvent.click(screen.getByTestId("typing-configure-button"));
    fireEvent.click(screen.getByTestId("typing-level-easy"));
    fireEvent.click(screen.getByTestId("game-config-start"));
    fireEvent.click(screen.getByTestId("typing-play"));
    await screen.findByTestId("typing-phase-typing");
    fireEvent.change(screen.getByTestId("typing-input"), {
      target: { value: "שלום" },
    });
    fireEvent.click(screen.getByTestId("typing-submit"));
    
    // Wait for feedback and verify next exercise button exists
    await waitFor(() => {
      expect(screen.getByText("pages.typingPractice.nextExercise")).toBeInTheDocument();
    });
  });
});