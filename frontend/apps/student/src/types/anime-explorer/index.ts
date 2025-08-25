export const AnimeType = {
  tv: "tv",
  movie: "movie",
  ova: "ova",
  special: "special",
  ona: "ona",
  music: "music",
  cm: "cm",
  pv: "pv",
  tv_special: "tv special",
} as const;

export type AnimeType = (typeof AnimeType)[keyof typeof AnimeType];

export interface AnimeItem {
  mal_id: number;
  url: string;
  images: {
    jpg: {
      image_url: string;
      small_image_url: string;
      large_image_url: string;
    };
  };
  title: string;
  title_english: string;
  title_japanese: string;
  type: AnimeType;
  episodes: number;
  status: string;
  duration: string;
  rating: string;
  score: number;
  synopsis: string;
  studios: Array<{
    mal_id: number;
    type: string;
    name: string;
    url: string;
  }>;
  genres: Array<{
    mal_id: number;
    type: string;
    name: string;
    url: string;
  }>;
}

export interface AnimeResponse {
  data: Array<AnimeItem>;
  pagination: {
    last_visible_page: number;
    has_next_page: boolean;
    current_page: number;
    items: {
      count: number;
      total: number;
      per_page: number;
    };
  };
}
