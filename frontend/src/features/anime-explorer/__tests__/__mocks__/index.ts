import { InfiniteData, UseInfiniteQueryResult } from '@tanstack/react-query';
import { vi } from 'vitest';

import { AnimeResponse } from '@/types';

vi.mock('react-i18next', () => ({
	useTranslation: () => ({ t: (k: string) => k }),
}));

export type UseGetAnimeSearchFn = (args: {
	search: string;
}) => UseInfiniteQueryResult<InfiniteData<AnimeResponse>, Error>;

export const useGetAnimeSearch = vi.fn<UseGetAnimeSearchFn>();

vi.mock('../../api', () => ({
	useGetAnimeSearch: (args: Parameters<UseGetAnimeSearchFn>[0]) =>
		useGetAnimeSearch(args),
}));

export let lastObserver: {
	cb: IntersectionObserverCallback;
	observe: (element: Element) => void;
	disconnect: () => void;
	el?: Element;
} | null;

export let lastIO: IntersectionObserver | null = null;

class IOStub implements IntersectionObserver {
	readonly root: Element | null = null;
	readonly rootMargin = '';
	readonly thresholds: ReadonlyArray<number> = [];
	private _cb: IntersectionObserverCallback;

	constructor(cb: IntersectionObserverCallback) {
		this._cb = cb;
		lastObserver = {
			cb,
			observe: (el) => (lastObserver!.el = el),
			disconnect: () => {},
			el: undefined,
		};
		// eslint-disable-next-line @typescript-eslint/no-this-alias
		lastIO = this;
	}
	observe(el: Element) {
		lastObserver!.observe(el);
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	unobserve(_el: Element) {}
	disconnect() {
		lastObserver!.disconnect();
	}
	takeRecords(): IntersectionObserverEntry[] {
		return [];
	}
}

(
	globalThis as unknown as { IntersectionObserver: typeof IntersectionObserver }
).IntersectionObserver = IOStub as unknown as typeof IntersectionObserver;

export const resetIO = () => {
	lastObserver = null;
	lastIO = null;
};

export const rq = (
	o: Partial<UseInfiniteQueryResult<InfiniteData<AnimeResponse>, Error>>
): UseInfiniteQueryResult<InfiniteData<AnimeResponse>, Error> =>
	o as unknown as UseInfiniteQueryResult<InfiniteData<AnimeResponse>, Error>;
