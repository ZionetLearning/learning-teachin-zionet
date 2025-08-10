import { useEffect, useRef, useState } from 'react';

import { useGetAnimeSearch } from './api';
import { AnimeCard } from './components';
import { useDebounceValue } from './utils';
import { useStyles } from './style';

export const AnimeExplorer = () => {
	const [search, setSearch] = useState('');
	const debouncedSearch = useDebounceValue(search, 600);

	const {
		data,
		isLoading,
		error,
		fetchNextPage,
		hasNextPage,
		isFetchingNextPage,
	} = useGetAnimeSearch({ search: debouncedSearch });

	const classes = useStyles({ isFetchingNextPage });

	const dataList = data?.pages.flatMap((page) => page.data) || [];
	const loadMoreRef = useRef<HTMLDivElement>(null);
	const listRef = useRef<HTMLDivElement>(null);

	useEffect(
		function loadMore() {
			if (!loadMoreRef.current || !hasNextPage || isFetchingNextPage) return;
			const observer = new IntersectionObserver(
				([entry]) => entry.isIntersecting && fetchNextPage()
			);
			observer.observe(loadMoreRef.current);
			return () => observer.disconnect();
		},
		[fetchNextPage, hasNextPage, isFetchingNextPage]
	);

	useEffect(
		function scrollToTopOnSearch() {
			listRef.current?.scrollTo({ top: 0, behavior: 'smooth' });
		},
		[debouncedSearch]
	);

	if (isLoading) return <p>Loading...</p>;
	if (error) return <p>Error fetching anime data: {error.message}</p>;

	return (
		<div className={classes.root}>
			<div className={classes.searchBar}>
				<input
					className={classes.searchInput}
					type="text"
					value={search}
					onChange={(e) => setSearch(e.target.value)}
					placeholder="Search for anime..."
				/>
				{search && (
					<button className={classes.clearButton} onClick={() => setSearch('')}>
						Clear
					</button>
				)}
			</div>
			<div ref={listRef} className={classes.list}>
				{!!dataList.length &&
					dataList.map((anime) => (
						<AnimeCard key={anime.mal_id} anime={anime} />
					))}
				{hasNextPage && <div ref={loadMoreRef} className={classes.sentinel} />}
				{<p className={classes.loadingMore}>Loading more...</p>}
			</div>
		</div>
	);
};
