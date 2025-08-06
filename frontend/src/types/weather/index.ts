export interface WeatherData {
	name: string;
	weather: Array<{ main: string; description: string; icon: string }>;
	main: {
		temp: number;
		feels_like: number;
		temp_min: number;
		temp_max: number;
		pressure: number;
		humidity: number;
	};
	wind: {
		speed: number;
		deg?: number;
		gust?: number;
	};
	visibility: number;
	sys: {
		sunrise: number;
		sunset: number;
	};
	timezone: number;
}

export type WeatherParams = { city: string } | { lat: number; lon: number };
