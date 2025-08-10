import { useState } from 'react';

import { Box, Card, CardMedia, Typography } from '@mui/material';
import StarIcon from '@mui/icons-material/Star';

import { AnimeItem } from '@/types';
import { useStyles } from './style';

export const AnimeCard = ({ anime }: { anime: AnimeItem }) => {
	const [hovered, setHovered] = useState(false);
	const classes = useStyles({ hovered });

	return (
		<Card
			className={classes.card}
			onMouseEnter={() => setHovered(true)}
			onMouseLeave={() => setHovered(false)}
		>
			<CardMedia
				component="img"
				image={anime.images.jpg.large_image_url}
				alt={anime.title}
				className={classes.media}
			/>
			<Typography variant="h6">{anime.title}</Typography>
			<Typography className={classes.rating}>
				<StarIcon className={classes.starIcon} />
				{anime.score}
			</Typography>
			<Typography className={classes.meta}>
				{anime.genres.map((genre) => genre.name).join(', ')}
			</Typography>
			<Typography
				variant="h6"
				className={classes.subtitle}
			>{`(${anime.title_japanese})`}</Typography>
			<Typography
				variant="body2"
				className={classes.meta}
			>{`Type: ${anime.type}`}</Typography>
			<Typography
				variant="body2"
				className={classes.meta}
			>{`Episodes: ${anime.episodes}`}</Typography>
			<Box className={classes.overlay}>{anime.synopsis}</Box>
		</Card>
	);
};
