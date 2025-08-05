import { ReactQueryContext } from '@/providers';
import { WeatherData, WeatherParams } from '@/types';
import { useQuery, UseQueryOptions } from '@tanstack/react-query';
import { useContext } from 'react';

export const useGetWeather = (
	params: WeatherParams,
	options?: UseQueryOptions<WeatherData, Error>
) => {
	const service = useContext(ReactQueryContext);

	if (!service) {
		throw new Error('useGetWeather must be used within a ReactQueryProvider');
	}

	const query = useQuery<WeatherData, Error>({
		queryKey: ['weather', params],
		queryFn: () =>
			'city' in params
				? service.getWeatherByCity(params.city)
				: service.getWeatherByCoordinates(params.lat, params.lon),
		...options,
		staleTime: 1000 * 60 * 5,
		retry: 1,
	});
	return query;
};
