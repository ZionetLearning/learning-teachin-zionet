import { useGetWeather } from '@/hooks';
import { WeatherParams } from '@/types';
import { useState } from 'react';
import { useStyles } from './style';

const emojiMap: Record<string, string> = {
	Clear: '☀️',
	Clouds: '⛅',
	Rain: '🌧️',
	Drizzle: '🌦️',
	Thunderstorm: '⚡',
	Snow: '❄️',
	Mist: '🌫️',
	Smoke: '🌫️',
	Haze: '🌫️',
	Dust: '🌫️',
	Fog: '🌫️',
	Sand: '🌫️',
	Ash: '🌋',
	Squall: '💨',
	Tornado: '🌪️',
};

const cities = [
	'New York',
	'London',
	'Tokyo',
	'Paris',
	'Berlin',
	'Sydney',
	'Rio de Janeiro',
	'Cape Town',
	'Mumbai',
];

export const WeatherWidget = () => {
	const classes = useStyles();
	const [selected, setSelected] = useState<string | null>(null);
	const [locating, setLocating] = useState(false);
	const [coords, setCoords] = useState<{ lat: number; lon: number } | null>(
		null
	);

	const params =
		selected === 'location'
			? coords
				? { lat: coords.lat, lon: coords.lon }
				: null
			: selected
				? { city: selected }
				: null;

	const { data, error, isLoading } = useGetWeather(params as WeatherParams, {
		queryKey: ['weather', params],
		enabled: !!params,
	});

	const handleSelectLocation = (value: string) => {
		setSelected(value || null);
		if (value === 'location' && navigator.geolocation) {
			setLocating(true);
			navigator.geolocation.getCurrentPosition(
				({ coords: { latitude, longitude } }) => {
					setCoords({ lat: latitude, lon: longitude });
					setLocating(false);
				},
				() => {
					setLocating(false);
					console.warn('Location access denied or not supported');
				}
			);
		}
	};

	const formatTime = (unix: number) =>
		new Date(unix * 1000).toLocaleTimeString([], {
			hour: '2-digit',
			minute: '2-digit',
		});

	return (
		<div className={classes.container}>
			<select
				className={classes.select}
				value={selected || ''}
				onChange={(e) => handleSelectLocation(e.target.value)}
			>
				<option value="">-- Select City or Use Location --</option>
				<option value="location">Use My Location</option>
				{cities.map((city) => (
					<option key={city} value={city}>
						{city}
					</option>
				))}
			</select>

			{locating && selected === 'location' && (
				<div className={classes.loading}>Locating...</div>
			)}
			{isLoading && !locating && (
				<div className={classes.loading}>Loading weather...</div>
			)}
			{error && (
				<div className={classes.error}>
					Error fetching weather data: {error.message}
				</div>
			)}

			{data && (
				<>
					<h3 className={classes.heading}>
						{data.weather[0].main && (
							<span className={classes.emoji}>
								{emojiMap[data.weather[0].main] || '🌡️'}
							</span>
						)}
						{selected === 'location' ? data.name : selected}
					</h3>
					<div className={classes.weatherContainer}>
						<div>
							<p className={classes.temp}>{Math.round(data.main.temp)}°C</p>
							<p className={classes.description}>
								{data.weather[0].description}
							</p>
						</div>
					</div>
					<div className={classes.stats}>
						Feels like: {Math.round(data.main.feels_like)}°C
						<br />
						Min: {Math.round(data.main.temp_min)}°C | Max:{' '}
						{Math.round(data.main.temp_max)}°C
						<br />
						Humidity: {data.main.humidity}% | Pressure: {data.main.pressure} hPa
						<br />
						Wind: {data.wind.speed} m/s
						{data.wind.deg ? ` at ${data.wind.deg}°` : ''}
						{data.wind.gust ? ` (gusts ${data.wind.gust} m/s)` : ''}
						<br />
						Visibility: {(data.visibility / 1000).toFixed(1)} km
						<br />
						Sunrise: {formatTime(data.sys.sunrise)} | Sunset:{' '}
						{formatTime(data.sys.sunset)}
					</div>
				</>
			)}
		</div>
	);
};
