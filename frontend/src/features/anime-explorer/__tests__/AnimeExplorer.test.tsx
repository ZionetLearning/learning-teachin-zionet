import { InfiniteData, UseInfiniteQueryResult } from '@tanstack/react-query';
import { act, fireEvent, render, screen } from '@testing-library/react';
import { vi } from 'vitest';

import { AnimeResponse } from '@/types';
import { AnimeExplorer } from '..';
import { useDebounceValue } from '../utils';

vi.mock('react-i18next', () => ({
	useTranslation: () => ({ t: (k: string) => k }),
}));

type UseGetAnimeSearchFn = (args: {
	search: string;
}) => UseInfiniteQueryResult<InfiniteData<AnimeResponse>, Error>;

const useGetAnimeSearch = vi.fn();
vi.mock('../api', () => ({
	useGetAnimeSearch: (args: Parameters<UseGetAnimeSearchFn>[0]) =>
		useGetAnimeSearch(args),
}));

let lastObserver: {
	cb: IntersectionObserverCallback;
	observe: (element: Element) => void;
	disconnect: () => void;
	el?: Element;
} | null;

class IOStub {
	cb: IntersectionObserverCallback;
	constructor(cb: IntersectionObserverCallback) {
		this.cb = cb;
		lastObserver = {
			cb,
			observe: (el) => (lastObserver!.el = el),
			disconnect: () => {},
			el: undefined,
		};
	}
	observe(el: Element) {
		lastObserver!.observe(el);
	}
	disconnect() {
		lastObserver!.disconnect();
	}
}
(
	globalThis as unknown as { IntersectionObserver: typeof IntersectionObserver }
).IntersectionObserver = IOStub as unknown as typeof IntersectionObserver;

beforeAll(() => {
	if (!HTMLElement.prototype.scrollTo) {
		Object.defineProperty(HTMLElement.prototype, 'scrollTo', {
			value: vi.fn(),
			writable: true,
			configurable: true,
		});
	}
});

const makeAnime = (
	over: Partial<AnimeResponse['data'][number]> = {}
): AnimeResponse['data'][number] => ({
	mal_id: 1,
	url: 'u',
	title: 'Fullmetal Alchemist: Brotherhood',
	title_english: 'FMAB',
	title_japanese: '鋼の錬金術師',
	type: 'tv' as const,
	episodes: 64,
	status: 'Finished',
	duration: '24 min',
	rating: 'PG-13',
	score: 9.1,
	synopsis: 'Two brothers, alchemy…',
	studios: [],
	genres: [{ mal_id: 1, type: 'anime', name: 'Action', url: 'g' }],
	images: {
		jpg: { image_url: '', small_image_url: '', large_image_url: 'img.jpg' },
	},
	...over,
});

const page = (
	data: AnimeResponse['data'],
	over: Partial<AnimeResponse['pagination']> = {}
): AnimeResponse => ({
	data,
	pagination: {
		current_page: 1,
		has_next_page: true,
		last_visible_page: 10,
		items: { count: data.length, total: data.length, per_page: 20 },
		...over,
	},
});

beforeEach(() => {
	vi.clearAllMocks();
	lastObserver = null;
	useGetAnimeSearch.mockReturnValue({
		data: { pages: [page([])], pageParams: [1] },
		isLoading: false,
		error: null,
		hasNextPage: false,
		isFetchingNextPage: false,
		fetchNextPage: vi.fn(),
	});
});

describe('<AnimeExplorer />', () => {
	it('matches snapshot', () => {
		const { asFragment } = render(<AnimeExplorer />);
		expect(asFragment()).toMatchSnapshot();
	});

	it('shows loading state', () => {
		useGetAnimeSearch.mockReturnValue({
			data: undefined,
			isLoading: true,
			error: null,
			hasNextPage: false,
			isFetchingNextPage: false,
			fetchNextPage: vi.fn(),
		});

		render(<AnimeExplorer />);
		expect(screen.getByText('pages.animeExplorer.loading')).toBeInTheDocument();
	});

	it('renders anime cards when data is present', () => {
		const anime1 = makeAnime({ mal_id: 1, title: 'Naruto' });
		const anime2 = makeAnime({ mal_id: 2, title: 'One Piece' });

		useGetAnimeSearch.mockReturnValue({
			data: { pages: [page([anime1, anime2])], pageParams: [1] },
			isLoading: false,
			error: null,
			hasNextPage: true,
			isFetchingNextPage: false,
			fetchNextPage: vi.fn(),
		});

		render(<AnimeExplorer />);

		expect(screen.getByText('Naruto')).toBeInTheDocument();
		expect(screen.getByText('One Piece')).toBeInTheDocument();
		expect(
			screen.getAllByRole('img', { name: /naruto|one piece/i })
		).toHaveLength(2);
		expect(screen.getAllByText(/^\s*Type:\s*tv\s*$/i)).toHaveLength(2);
		expect(screen.getAllByText(/^\s*Episodes:\s*64\s*$/i)).toHaveLength(2);
	});

	it('triggers fetchNextPage when sentinel intersects', () => {
		const anime = makeAnime({ title: 'Jujutsu Kaisen' });
		const fetchNextPage = vi.fn();

		useGetAnimeSearch.mockReturnValue({
			data: { pages: [page([anime])], pageParams: [1] },
			isLoading: false,
			error: null,
			hasNextPage: true,
			isFetchingNextPage: false,
			fetchNextPage,
		});

		render(<AnimeExplorer />);

		expect(lastObserver).toBeTruthy();
		lastObserver!.cb(
			[
				{
					isIntersecting: true,
					target: lastObserver!.el!,
				} as IntersectionObserverEntry,
			],
			{} as IntersectionObserver
		);
		expect(fetchNextPage).toHaveBeenCalled();
	});

	it('shows reach-end message when no more pages', () => {
		const anime = makeAnime({ title: 'Attack on Titan' });

		useGetAnimeSearch.mockReturnValue({
			data: {
				pages: [page([anime], { has_next_page: false })],
				pageParams: [1],
			},
			isLoading: false,
			error: null,
			hasNextPage: false,
			isFetchingNextPage: false,
			fetchNextPage: vi.fn(),
		});

		render(<AnimeExplorer />);

		expect(
			screen.getByText('pages.animeExplorer.reachEnd')
		).toBeInTheDocument();
	});

	it('updates search via debounce after 600ms', () => {
		vi.useFakeTimers();
		render(<AnimeExplorer />);

		const input = screen.getByRole('textbox', {
			name: /search anime/i,
		}) as HTMLInputElement;

		fireEvent.change(input, { target: { value: 'Naruto' } });

		let last = useGetAnimeSearch.mock.calls.at(-1)?.[0]?.search ?? '';
		expect(last).toBe('');

		act(() => {
			vi.advanceTimersByTime(599);
		});
		last = useGetAnimeSearch.mock.calls.at(-1)?.[0]?.search ?? '';
		expect(last).toBe('');

		act(() => {
			vi.advanceTimersByTime(1);
		});
		last = useGetAnimeSearch.mock.calls.at(-1)?.[0]?.search ?? '';
		expect(last).toBe('Naruto');

		vi.useRealTimers();
	});

	it('clicking "Back to top" calls scrollTo on the list container', () => {
		const scrollToSpy = vi
			.spyOn(HTMLElement.prototype, 'scrollTo')
			.mockImplementation(() => {});
		render(<AnimeExplorer />);

		fireEvent.click(screen.getByRole('button', { name: /back to top/i }));
		expect(scrollToSpy).toHaveBeenCalledWith({ top: 0, behavior: 'smooth' });

		scrollToSpy.mockRestore();
	});
});

const Harness = ({ value, delay = 300 }: { value: string; delay?: number }) => {
	const debounced = useDebounceValue(value, delay);
	return <div data-testid="val">{debounced}</div>;
};

describe('useDebounceValue', () => {
	beforeEach(() => {
		vi.useFakeTimers();
	});
	afterEach(() => {
		vi.useRealTimers();
	});

	it('returns the initial value immediately', () => {
		render(<Harness value="a" />);
		expect(screen.getByTestId('val')).toHaveTextContent('a');
	});

	it('updates after the default delay (300ms)', () => {
		const { rerender } = render(<Harness value="a" />);
		rerender(<Harness value="b" />);

		expect(screen.getByTestId('val')).toHaveTextContent('a');
		act(() => vi.advanceTimersByTime(299));
		expect(screen.getByTestId('val')).toHaveTextContent('a');

		act(() => vi.advanceTimersByTime(1));
		expect(screen.getByTestId('val')).toHaveTextContent('b');
	});

	it('respects a custom delay', () => {
		const { rerender } = render(<Harness value="a" delay={500} />);
		rerender(<Harness value="c" delay={500} />);

		act(() => vi.advanceTimersByTime(499));
		expect(screen.getByTestId('val')).toHaveTextContent('a');

		act(() => vi.advanceTimersByTime(1));
		expect(screen.getByTestId('val')).toHaveTextContent('c');
	});

	it('only applies the latest value if changes happen rapidly', () => {
		const { rerender } = render(<Harness value="a" />);
		rerender(<Harness value="b" />);
		act(() => vi.advanceTimersByTime(150));
		rerender(<Harness value="c" />);

		act(() => vi.advanceTimersByTime(299));
		expect(screen.getByTestId('val')).toHaveTextContent('a');

		act(() => vi.advanceTimersByTime(1));
		expect(screen.getByTestId('val')).toHaveTextContent('c');
	});
});
