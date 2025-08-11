import { render, screen, fireEvent } from '@testing-library/react';
import { vi, describe, it, expect } from 'vitest';
import { ChatYo } from '../';

// spy-able sendMessage
const sendMessageMock = vi.fn();

vi.mock('react-i18next', () => ({
	useTranslation: () => ({ t: (k: string) => k }),
}));

// hook mock
vi.mock('@/hooks', () => ({
	useChat: () => ({
		sendMessage: sendMessageMock,
		loading: false,
		messages: [
			{ text: 'Hello', position: 'right', date: new Date() },
			{ text: 'Hi there!', position: 'left', date: new Date() },
		],
	}),
}));

// mock ChatUi to render a minimal input and button that call the passed props
vi.mock('@/components', () => ({
	ReactChatElements: ({
		messages,
		loading,
		value,
		onChange,
		handleSendMessage,
	}: {
		messages: Array<{ text: string }>;
		loading: boolean;
		value: string;
		onChange: (v: string) => void;
		handleSendMessage: () => void;
	}) => (
		<div>
			<ul>
				{messages.map((m, i) => (
					<li key={i}>{m.text}</li>
				))}
			</ul>
			{loading && <div>Loading…</div>}
			<input
				placeholder="Type a message..."
				value={value}
				onChange={(e) => onChange((e.target as HTMLInputElement).value)}
			/>
			<button type="button" onClick={handleSendMessage}>
				↑
			</button>
		</div>
	),
}));

describe('ChatYo', () => {
	it('types, sends, clears input, and calls sendMessage', () => {
		sendMessageMock.mockReset();

		render(<ChatYo />);

		// sees existing messages
		expect(screen.getByText('Hello')).toBeInTheDocument();
		expect(screen.getByText('Hi there!')).toBeInTheDocument();

		const input = screen.getByPlaceholderText(
			'Type a message...'
		) as HTMLInputElement;

		fireEvent.change(input, { target: { value: 'Test message' } });
		expect(input.value).toBe('Test message');

		fireEvent.click(screen.getByRole('button', { name: '↑' }));

		expect(sendMessageMock).toHaveBeenCalledTimes(1);
		expect(sendMessageMock).toHaveBeenCalledWith('Test message');
		expect(input.value).toBe(''); // cleared by ChatYo after send
	});

	it('matches snapshot', () => {
		const { asFragment } = render(<ChatYo />);
		expect(asFragment()).toMatchSnapshot();
	});
});
