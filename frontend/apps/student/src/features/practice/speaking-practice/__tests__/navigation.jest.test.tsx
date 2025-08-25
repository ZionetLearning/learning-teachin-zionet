import "./__mocks__";
import { SpeakingPractice } from "@/features/practice/speaking-practice";
import { render, fireEvent } from "@testing-library/react";

jest.mock("microsoft-cognitiveservices-speech-sdk");

describe("<SpeakingPractice /> navigation", () => {
  it("wraps backward from first to last", () => {
    const { getByText, getByRole } = render(<SpeakingPractice />);

    const prev = getByRole("button", { name: /Prev/i });
    expect(getByText(/1 \/ 12/i)).toBeInTheDocument();
    fireEvent.click(prev);
    expect(getByText(/12 \/ 12/i)).toBeInTheDocument();
  });

  it("wraps forward from last to first", () => {
    const { getByText, getByRole } = render(<SpeakingPractice />);
    const next = getByRole("button", { name: /Next/i });

    for (let i = 0; i < 12; i++) {
      fireEvent.click(next);
    }

    expect(getByText(/1 \/ 12/i)).toBeInTheDocument();
  });
});
