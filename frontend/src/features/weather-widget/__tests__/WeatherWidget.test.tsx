import { vi } from 'vitest';
import { WeatherWidget } from '..';
import { useGetWeather } from '../api';
import { WeatherData, WeatherParams } from '@/types';
import type { UseQueryResult } from '@tanstack/react-query';
import { fireEvent, render, screen } from '@testing-library/react';

vi.mock('react-i18next', () => ({
	useTranslation: () => ({ t: (k: string) => k }),
}));

vi.mock('../api', () => {
	return {
		useGetWeather: vi.fn(),
	};
});

const sampleData: WeatherData = {
	name: 'London',
	timezone: 0,
	weather: [{ main: 'Rain', icon: '10d', description: 'light rain' }],
	main: {
		temp: 12.6,
		feels_like: 11.9,
		temp_min: 10.1,
		temp_max: 13.9,
		humidity: 76,
		pressure: 1014,
	},
	wind: { speed: 3.2, deg: 240, gust: 5.1 },
	visibility: 9000,
	sys: { sunrise: 1672531199, sunset: 1672574399 },
};

type QR = UseQueryResult<WeatherData, Error>;

const emptyResult: QR = {
	data: undefined,
	error: null,
	isLoading: false,
} as QR;

describe('<WeatherWidget />', () => {
	beforeEach(() => {
		vi.clearAllMocks();
		vi.useRealTimers();
		vi.mocked(useGetWeather).mockImplementation((params: unknown) => {
			const p = params as WeatherParams;
			if (p && 'city' in p && p.city === 'pages.weather.cities.london') {
				return {
					data: sampleData,
					error: null,
					isLoading: false,
				} as UseQueryResult<WeatherData, Error>;
			}
			if (p && 'city' in p && p.city === 'pages.weather.cities.tokyo') {
				return {
					data: undefined,
					error: new Error('city not found'),
					isLoading: false,
				} as UseQueryResult<WeatherData, Error>;
			}
			if (p && 'lat' in p && 'lon' in p) {
				return {
					data: sampleData,
					error: null,
					isLoading: false,
				} as UseQueryResult<WeatherData, Error>;
			}
			return emptyResult;
		});
	});

	it('shows the initial prompt before a city is selected/searched', () => {
		render(<WeatherWidget />);
		expect(
			screen.getByText('pages.weather.selectCityOrLocation')
		).toBeInTheDocument();

		expect(screen.getByRole('combobox', { name: '' })).toBeInTheDocument();
		expect(
			screen.getByPlaceholderText('pages.weather.searchCity')
		).toBeInTheDocument();
		expect(
			screen.getByRole('button', { name: 'pages.weather.search' })
		).toBeInTheDocument();
	});

	it('selecting a preset city calls the hook with city and renders data', async () => {
		render(<WeatherWidget />);

		fireEvent.change(screen.getByRole('combobox'), {
			target: { value: 'pages.weather.cities.london' },
		});

		await screen.findByRole('heading', { level: 3, name: 'London' });
		expect(screen.getByText('light rain')).toBeInTheDocument();

		const calls = vi.mocked(useGetWeather).mock.calls;
		expect(
			calls.some(
				([p]) => p && 'city' in p && p.city === 'pages.weather.cities.london'
			)
		).toBe(true);
	});

	it('geolocation error falls back to initial prompt', async () => {
		const error: GeolocationPositionError = {
			code: 1,
			message: 'denied',
			PERMISSION_DENIED: 1,
			POSITION_UNAVAILABLE: 2,
			TIMEOUT: 3,
		};
		const getCurrentPosition = vi.fn(
			(_ok: PositionCallback, err?: PositionErrorCallback) => {
				err?.(error);
			}
		);
		Object.defineProperty(global.navigator, 'geolocation', {
			value: { getCurrentPosition },
			configurable: true,
		});

		render(<WeatherWidget />);
		fireEvent.change(screen.getByRole('combobox'), {
			target: { value: 'location' },
		});
		expect(getCurrentPosition).toHaveBeenCalled();

		await screen.findByText('pages.weather.selectCityOrLocation');
	});

	it('shows a message for "city not found" error', async () => {
		render(<WeatherWidget />);
		fireEvent.change(screen.getByRole('combobox'), {
			target: { value: 'pages.weather.cities.tokyo' },
		});
		await screen.findByText('City not found. Please try again.');
	});

	it('renders sunrise/sunset times (formatted)', async () => {
		render(<WeatherWidget />);
		fireEvent.change(screen.getByRole('combobox'), {
			target: { value: 'pages.weather.cities.london' },
		});

		const statsContainer = await screen.findByText(/pages\.weather\.sunrise/i, {
			exact: false,
		});
		expect(statsContainer).toHaveTextContent(
			/pages\.weather\.sunrise\s+\d{1,2}:\d{2}/i
		);
		expect(statsContainer).toHaveTextContent(
			/pages\.weather\.sunset\s*\d{1,2}:\d{2}/i
		);
	});
});
