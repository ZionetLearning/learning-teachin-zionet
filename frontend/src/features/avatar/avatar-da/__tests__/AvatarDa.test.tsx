import "./__mocks__";

import { vi } from "vitest";

import "@testing-library/jest-dom/vitest";
import { fireEvent, render, screen } from "@testing-library/react";

import { mockSpeak } from "./__mocks__";

vi.stubEnv("VITE_AZURE_SPEECH_KEY", "mock-speech-key");
vi.stubEnv("VITE_AZURE_REGION", "mock-region");

import { AvatarDa } from "..";

describe("<AvatarDa />", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("mounts with Canvas + input + Speak button", () => {
    render(<AvatarDa />);
    expect(screen.getByTestId("canvas")).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText("Write something here in Hebrew"),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Speak" })).toBeInTheDocument();
  });

  it("does nothing when Speak is clicked with empty input", () => {
    render(<AvatarDa />);
    fireEvent.click(screen.getByRole("button", { name: "Speak" }));
    expect(mockSpeak).not.toHaveBeenCalled();
  });

  it("calls speak function when button is clicked with text input", () => {
    render(<AvatarDa />);
    const input = screen.getByPlaceholderText("Write something here in Hebrew");
    const btn = screen.getByRole("button", { name: "Speak" });

    fireEvent.change(input, { target: { value: "שלום" } });
    fireEvent.click(btn);

    expect(mockSpeak).toHaveBeenCalledWith("שלום");
  });
});
