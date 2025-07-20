/* eslint-disable @typescript-eslint/no-explicit-any */
/** MockEventSource simulates a Server-Sent Events (SSE) connection with the same shape as the native EventSource */
export class MockEventSource {
	// connection properties
	public readonly url: string;
	public readonly withCredentials: boolean;
	public readyState: MockEventSourceState = MockEventState.CONNECTING;

	// event hooks
	public onopen: ((ev: Event) => void) | null = null;
	public onmessage: ((ev: MessageEvent) => void) | null = null;
	public onerror: ((ev: Event) => void) | null = null;

	public readonly CONNECTING = MockEventState.CONNECTING;
	public readonly OPEN = MockEventState.OPEN;
	public readonly CLOSED = MockEventState.CLOSED;

	constructor(url: string, init?: EventSourceInit) {
		this.url = url;
		this.withCredentials = !!init?.withCredentials;

		setTimeout(() => {
			this.readyState = MockEventState.OPEN;
			this.onopen?.(new Event('open')); // Notify that the connection is open
			this.emitTokens();
		}, 100);
	}

	private emitTokens() {
		if (this.readyState !== MockEventState.OPEN) return;

		const tokens = [
			'Hello, ',
			'this is a mock ',
			'event source. ',
			'These are ',
			'mock tokens ',
			'hardcoded for ',
			'testing purposes. ',
			'You can replace ',
			'them with your own ',
			'mock data. ',
			'Until then, ',
			'enjoy the mock ',
			'experience!',
		];
		let i = 0;

		const interval = setInterval(() => {
			if (i === tokens.length) {
				clearInterval(interval);
				const doneData = JSON.stringify({ text: '', done: true });
				this.onmessage?.(new MessageEvent('message', { data: doneData }));
				this.close();
				return;
			}
			const data = JSON.stringify({ text: tokens[i++] });
			this.onmessage?.(new MessageEvent('message', { data }));
		}, 300);
	}

	addEventListener(
		type: 'open' | 'message' | 'error',
		listener: (ev: any) => void
	) {
		if (type === 'open') this.onopen = listener;
		if (type === 'message') this.onmessage = listener;
		if (type === 'error') this.onerror = listener;
	}

	removeEventListener(
		type: 'open' | 'message' | 'error',
		listener: (ev: any) => void
	) {
		if (type === 'open' && this.onopen === listener) this.onopen = null;
		if (type === 'message' && this.onmessage === listener)
			this.onmessage = null;
		if (type === 'error' && this.onerror === listener) this.onerror = null;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	dispatchEvent(_ev: Event): boolean {
		return false;
	}

	close() {
		this.readyState = MockEventState.CLOSED;
	}
}

export const MockEventState = {
	CONNECTING: 0,
	OPEN: 1,
	CLOSED: 2,
} as const;
export type MockEventSourceState =
	(typeof MockEventState)[keyof typeof MockEventState];
