import "./__mocks__";
import { SpeakingPractice } from "@student/features/practice/speaking-practice";
import { fireEvent, render } from "@testing-library/react";

jest.mock("microsoft-cognitiveservices-speech-sdk");

describe("<SpeakingPractice /> Nikud toggle", () => {
  it("toggles Nikud display when button clicked", () => {
    const { getByRole } = render(<SpeakingPractice />);
    const nikudButton = getByRole("button", {
      name: "pages.speakingPractice.showNikud",
    });
    expect(nikudButton).toBeInTheDocument();

    fireEvent.click(nikudButton);
    expect(
      getByRole("button", { name: "pages.speakingPractice.hideNikud" }),
    ).toBeInTheDocument();

    fireEvent.click(
      getByRole("button", { name: "pages.speakingPractice.hideNikud" }),
    );
    expect(
      getByRole("button", { name: "pages.speakingPractice.showNikud" }),
    ).toBeInTheDocument();
  });

  it("displays different text with and without Nikud", () => {
    const { getByRole } = render(<SpeakingPractice />);

    expect(getByRole("heading")).toHaveTextContent("שלום");
    fireEvent.click(
      getByRole("button", { name: "pages.speakingPractice.showNikud" }),
    );
    expect(getByRole("heading")).toHaveTextContent("שָׁלוֹם");
  });
});
