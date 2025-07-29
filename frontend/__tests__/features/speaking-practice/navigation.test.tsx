import { SpeakingPractice } from '@/features/speaking-practice';
import { render, fireEvent } from '@testing-library/react';

jest.mock('microsoft-cognitiveservices-speech-sdk');

describe('<SpeakingPractice /> navigation', () => {
	it('wraps backward from first to last', () => {
		const { getByText } = render(<SpeakingPractice />);
		const prev = getByText(/Prev/i);
		expect(getByText(/1 \/ 12/i)).toBeInTheDocument();
		fireEvent.click(prev);
		expect(getByText(/12 \/ 12/i)).toBeInTheDocument();
	});

	it('wraps forward from last to first', () => {
		const { getByText } = render(<SpeakingPractice />);
		const next = getByText(/Next/i);

		for (let i = 0; i < 12; i++) {
			fireEvent.click(next);
		}

		expect(getByText(/1 \/ 12/i)).toBeInTheDocument();
	});
});
