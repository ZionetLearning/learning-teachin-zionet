import "./__mocks__";

import { vi } from "vitest";

import "@testing-library/jest-dom/vitest";
import { fireEvent, render, screen } from "@testing-library/react";

import { mockSpeak } from "./__mocks__";

import { AvatarDa } from "..";

describe("<AvatarDa />", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("mounts with Canvas + input + Speak button", () => {
    render(<AvatarDa />);
    expect(screen.getByTestId("canvas")).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText("pages.avatarDa.writeSomethingHereInHebrew"),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "pages.avatarDa.speak" }),
    ).toBeInTheDocument();
  });

  it("does nothing when Speak is clicked with empty input", () => {
    render(<AvatarDa />);
    fireEvent.click(
      screen.getByRole("button", { name: "pages.avatarDa.speak" }),
    );
    expect(mockSpeak).not.toHaveBeenCalled();
  });

  it("calls speak function when button is clicked with text input", () => {
    render(<AvatarDa />);
    const input = screen.getByPlaceholderText(
      "pages.avatarDa.writeSomethingHereInHebrew",
    );
    const btn = screen.getByRole("button", { name: "pages.avatarDa.speak" });

    fireEvent.change(input, { target: { value: "שלום" } });
    fireEvent.click(btn);

    expect(mockSpeak).toHaveBeenCalledWith("שלום");
  });
});
