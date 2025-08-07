import { useEffect, useRef } from 'react';
import { useGetAnimeSearch } from './api';
import { AnimeCard } from './components';

export const AnimeExplorer = () => {
	const {
		data,
		isLoading,
		error,
		fetchNextPage,
		hasNextPage,
		isFetchingNextPage,
	} = useGetAnimeSearch();

	const dataList = data?.pages.flatMap((page) => page.data) || [];
	const loadMoreRef = useRef<HTMLDivElement>(null);

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

	if (isLoading) return <p>Loading...</p>;
	if (error) return <p>Error fetching anime data: {error.message}</p>;

	return (
		<div
			style={{
				height: '100%',
				display: 'flex',
				flexDirection: 'column',
				boxSizing: 'border-box',
				backgroundColor: '#f0f0f0',
			}}
		>
			<div
				style={{
					flex: 1,
					overflowY: 'auto',
					padding: '20px',
					boxSizing: 'border-box',
					display: 'flex',
					flexWrap: 'wrap',
					gap: '20px',
					justifyContent: 'center',
				}}
			>
				{!!dataList.length &&
					dataList.map((anime) => (
						<AnimeCard key={anime.mal_id} anime={anime} />
					))}
				{hasNextPage && (
					<div ref={loadMoreRef} style={{ height: 1, width: '100%' }} />
				)}
				{
					<p
						style={{
							color: '#000',
							visibility: isFetchingNextPage ? 'visible' : 'hidden',
						}}
					>
						Loading more...
					</p>
				}
			</div>
		</div>
	);
};
