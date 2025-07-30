import { SpeakingPractice } from "@/features/speaking-practice";
import { fireEvent, render } from "@testing-library/react";

jest.mock("microsoft-cognitiveservices-speech-sdk");

describe("<SpeakingPractice /> Nikud toggle", () => {
  it("toggles Nikud display when button clicked", () => {
    const { getByRole } = render(<SpeakingPractice />);
    const nikudButton = getByRole("button", { name: "Show Nikud" });
    expect(nikudButton).toBeInTheDocument();

    fireEvent.click(nikudButton);
    expect(getByRole("button", { name: "Hide Nikud" })).toBeInTheDocument();

    fireEvent.click(getByRole("button", { name: "Hide Nikud" }));
    expect(getByRole("button", { name: "Show Nikud" })).toBeInTheDocument();
  });

  it("displays different text with and without ikud", () => {
    const { getByRole } = render(<SpeakingPractice />);

    expect(getByRole("heading")).toHaveTextContent("שלום");
    fireEvent.click(getByRole("button", { name: "Show Nikud" }));
    expect(getByRole("heading")).toHaveTextContent("שָׁלוֹם");
  });
});
