import { SpeakingPractice } from "@/features/practice/speaking-practice";
import { fireEvent, render, waitFor } from "@testing-library/react";

jest.mock("microsoft-cognitiveservices-speech-sdk");
const mockedSdk = jest.requireMock(
  "microsoft-cognitiveservices-speech-sdk",
) as typeof import("microsoft-cognitiveservices-speech-sdk");

describe("<SpeakingPractice /> speech recognition", () => {
  it("shows perfect feedback on correct recognition", async () => {
    const mockRecognizer = {
      recognizeOnceAsync: jest.fn(
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        (onSuccess, _onError) => onSuccess({ text: "שלום" }),
      ),
      close: jest.fn(),
    } as unknown as typeof mockedSdk.SpeechRecognizer;

    (mockedSdk.SpeechRecognizer as unknown as jest.Mock).mockImplementation(
      () => mockRecognizer,
    );

    const { getByText, getByRole } = render(<SpeakingPractice />);
    fireEvent.click(getByRole("button", { name: /record/i }));

    await waitFor(() => {
      expect(getByText("Perfect!")).toBeInTheDocument();
    });
  });

  it("shows try again feedback on incorrect recognition", async () => {
    const mockRecognizer = {
      recognizeOnceAsync: jest.fn(
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        (onSuccess, _onError) => onSuccess({ text: "שלום עולם" }),
      ),
      close: jest.fn(),
    } as unknown as typeof mockedSdk.SpeechRecognizer;

    (mockedSdk.SpeechRecognizer as unknown as jest.Mock).mockImplementation(
      () => mockRecognizer,
    );

    const { getByText, getByRole } = render(<SpeakingPractice />);

    fireEvent.click(getByRole("button", { name: /record/i }));

    await waitFor(() => {
      expect(
        getByText("Try again, that was not accurate."),
      ).toBeInTheDocument();
    });
  });

  it("shows recognition error feedback when the SDK errors", async () => {
    const errorSpy = jest.spyOn(console, "error").mockImplementation(() => {});

    const mockRecognizer = {
      recognizeOnceAsync: jest.fn((_onSuccess, onError) =>
        onError(new Error("simulated failure")),
      ),
      close: jest.fn(),
    } as unknown as typeof mockedSdk.SpeechRecognizer;

    (mockedSdk.SpeechRecognizer as unknown as jest.Mock).mockImplementation(
      () => mockRecognizer,
    );

    const { getByText, getByRole } = render(<SpeakingPractice />);
    fireEvent.click(getByRole("button", { name: /record/i }));

    await waitFor(() => {
      expect(getByText("Speech recognition error.")).toBeInTheDocument();
    });

    errorSpy.mockRestore();
  });
});
