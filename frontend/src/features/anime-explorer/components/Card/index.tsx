import { useState } from "react";

import StarIcon from "@mui/icons-material/Star";
import { Box, Card, Typography } from "@mui/material";

import { AnimeItem } from "@/types";

import { useStyles } from "./style";

export const AnimeCard = ({ anime }: { anime: AnimeItem }) => {
  const [hovered, setHovered] = useState(false);
  const classes = useStyles({ hovered });

  const genres = anime.genres?.map((g) => g.name).join(", ") || "—";
  const type = anime.type || "—";
  const episodes =
    typeof anime.episodes === "number" ? String(anime.episodes) : "—";
  const score = typeof anime.score === "number" ? anime.score : "—";

  return (
    <Card
      className={classes.card}
      onMouseEnter={() => setHovered(true)}
      onMouseLeave={() => setHovered(false)}
    >
      <img
        src={anime.images.jpg.large_image_url}
        alt={anime.title}
        className={classes.media}
        loading="lazy"
      />
      <Typography variant="h6" className={classes.titleClamp}>
        {anime.title}
      </Typography>
      <Box className={classes.rating}>
        <StarIcon className={classes.starIcon} />
        {score}
      </Box>
      <Typography className={classes.genresClamp}>{genres}</Typography>
      <Box className={classes.spacer} aria-hidden />
      <Typography variant="body2" className={classes.meta}>
        {`Type: ${type}`}
      </Typography>
      <Typography variant="body2" className={classes.meta}>
        {`Episodes: ${episodes}`}
      </Typography>
      <Box className={classes.overlay}>
        <Typography className={classes.synopsisClamp}>
          {anime.synopsis}
        </Typography>
      </Box>
    </Card>
  );
};
