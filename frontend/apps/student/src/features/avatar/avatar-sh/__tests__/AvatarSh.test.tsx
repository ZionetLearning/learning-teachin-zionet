import { render, screen, fireEvent } from "@testing-library/react";
import { vi, describe, it, expect, beforeEach } from "vitest";
import { AvatarSh } from "../";

//Mocks
vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (k: string) => k }),
}));

const speakMock = vi.fn();
vi.mock("@student/hooks", () => ({
  useAvatarSpeech: () => ({
    currentVisemeSrc: "viseme.svg",
    speak: speakMock,
  }),
}));

describe("AvatarSh", () => {
  beforeEach(() => {
    speakMock.mockReset();
  });

  it("renders avatar and lips images, allows typing, and calls speak on click", () => {
    render(<AvatarSh />);

    // Images exist
    const avatarImg = screen.getByAltText("Avatar") as HTMLImageElement;
    const lipsImg = screen.getByAltText("Lips") as HTMLImageElement;
    expect(avatarImg).toBeInTheDocument();
    expect(lipsImg).toBeInTheDocument();
    expect(avatarImg.src).toContain("avatar.svg");
    expect(lipsImg.src).toContain("viseme.svg");

    // Input uses i18n key as placeholder (we mocked t to return the key)
    const input = screen.getByPlaceholderText(
      "pages.avatarSh.writeSomethingHereInHebrew",
    ) as HTMLInputElement;
    expect(input).toHaveAttribute("dir", "rtl");

    // Type and click Speak
    fireEvent.change(input, { target: { value: "שלום" } });
    expect(input.value).toBe("שלום");

    const speakBtn = screen.getByRole("button", {
      name: "pages.avatarSh.speak",
    });
    fireEvent.click(speakBtn);

    expect(speakMock).toHaveBeenCalledTimes(1);
    expect(speakMock).toHaveBeenCalledWith("שלום");
  });
});
