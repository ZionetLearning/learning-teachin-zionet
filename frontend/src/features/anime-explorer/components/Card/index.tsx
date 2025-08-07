import { useState } from 'react';

import { Box, Card, CardMedia, Collapse, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

import { AnimeItem } from '@/types';
import { ExpandMore } from '../ExpandMore';

export const AnimeCard = ({ anime }: { anime: AnimeItem }) => {
	const [expanded, setExpanded] = useState(false);
	return (
		<Card
			sx={{
				width: 220,
				display: 'flex',
				flexDirection: 'column',
				alignItems: 'center',
				gap: '10px',
				border: '1px solid #ccc',
				padding: '10px',
				borderRadius: '8px',
			}}
		>
			<CardMedia
				component="img"
				image={anime.images.jpg.large_image_url}
				alt={anime.title}
				sx={{ width: '150px', height: 'auto', borderRadius: '8px' }}
			/>
			<Typography variant="h6">{anime.title}</Typography>
			<Typography variant="h6">{`(${anime.title_japanese})`}</Typography>
			<Typography variant="body2">{`Type: ${anime.type}`}</Typography>
			<Typography variant="body2">{`Episodes: ${anime.episodes}`}</Typography>
			<ExpandMore
				expand={expanded}
				onClick={() => setExpanded(!expanded)}
				aria-expanded={expanded}
				aria-label="show more"
			>
				<ExpandMoreIcon />
			</ExpandMore>
			<Collapse in={expanded} timeout="auto" unmountOnExit>
				<Box
					sx={{
						wordBreak: 'break-word',
						textAlign: 'center',
						mt: 1,
					}}
				>
					<Typography variant="body2">{anime.synopsis}</Typography>
				</Box>
			</Collapse>
		</Card>
	);
};
