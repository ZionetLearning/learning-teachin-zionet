import "./__mocks__";
import { SpeakingPractice } from "@student/features/practice/speaking-practice";
import { render } from "@testing-library/react";

jest.mock("microsoft-cognitiveservices-speech-sdk");

describe("<SpeakingPractice /> snapshot", () => {
  it("renders initial UI correctly", () => {
    const { container } = render(<SpeakingPractice />);

    expect(container).toMatchSnapshot();
  });
});
