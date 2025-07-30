import { SpeakingPractice } from "@/features/speaking-practice";
import { fireEvent, render, waitFor } from "@testing-library/react";

jest.mock("microsoft-cognitiveservices-speech-sdk");

describe("<SpeakingPractice /> interactions", () => {
  it("shows “▶ Play” then “⏹ Stop” when clicked", () => {
    const { getByRole } = render(<SpeakingPractice />);

    const playButton = getByRole("button", { name: /play/i });
    fireEvent.click(playButton);
    expect(getByRole("button", { name: /stop/i })).toBeInTheDocument();
  });

  it("toggles to “⏹ Stop” while speaking and back to “▶ Play” when done", async () => {
    const { getByRole } = render(<SpeakingPractice />);

    fireEvent.click(getByRole("button", { name: /play/i }));
    expect(getByRole("button", { name: /stop/i })).toBeInTheDocument();

    await waitFor(() => {
      expect(getByRole("button", { name: /play/i })).toBeInTheDocument();
    });
  });

  it("allows manually stopping mid-synthesis", () => {
    const { getByRole } = render(<SpeakingPractice />);

    fireEvent.click(getByRole("button", { name: /play/i }));
    expect(getByRole("button", { name: /stop/i })).toBeInTheDocument();

    fireEvent.click(getByRole("button", { name: /stop/i }));
    expect(getByRole("button", { name: /play/i })).toBeInTheDocument();
  });
});
