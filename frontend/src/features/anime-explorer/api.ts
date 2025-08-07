import { AnimeResponse } from '@/types';
import {
	InfiniteData,
	useInfiniteQuery,
	UseInfiniteQueryResult,
} from '@tanstack/react-query';

const getAnimeSearch = async ({
	page = 1,
}: {
	page?: number;
}): Promise<AnimeResponse> => {
	try {
		const response = await fetch(
			`https://api.jikan.moe/v4/anime?page=${page}&limit=20`
		);
		const payload = await response.json();
		if (!response.ok) {
			throw new Error(payload.message || 'Failed to fetch anime data');
		}
		return payload as AnimeResponse;
	} catch (error) {
		console.warn('Error fetching anime data:', error);
		throw error;
	}
};

export function useGetAnimeSearch(): UseInfiniteQueryResult<
	InfiniteData<AnimeResponse>,
	Error
> {
	return useInfiniteQuery<
		AnimeResponse,
		Error,
		InfiniteData<AnimeResponse>,
		string[],
		number
	>({
		queryKey: ['animeSearch'],
		queryFn: ({ pageParam }) => getAnimeSearch({ page: pageParam }),
		getNextPageParam: (lastPage) =>
			lastPage.pagination.has_next_page
				? lastPage.pagination.current_page + 1
				: undefined,
		initialPageParam: 1,
		staleTime: 1000 * 60 * 5,
		retry: 1,
	});
}
