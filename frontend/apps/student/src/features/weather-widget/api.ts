import {
  useQuery,
  UseQueryOptions,
  UseQueryResult,
} from "@tanstack/react-query";

import { WeatherData, WeatherParams } from "@/types";

const getWeatherByCoordinates = async (
  lat: number,
  lon: number,
): Promise<WeatherData> => {
  try {
    const response = await fetch(
      `https://api.openweathermap.org/data/2.5/weather?lat=${lat}&lon=${lon}&units=metric&appid=${import.meta.env.VITE_OPENWEATHERMAP_API_KEY}`,
    );
    const payload = await response.json();
    if (!response.ok) {
      throw new Error(payload.message || "Failed to fetch weather data");
    }
    return payload as WeatherData;
  } catch (error) {
    console.warn("Error fetching weather data:", error);
    throw error;
  }
};

const getWeatherByCity = async (city: string): Promise<WeatherData> => {
  try {
    const response = await fetch(
      `https://api.openweathermap.org/data/2.5/weather?q=${city}&units=metric&appid=${import.meta.env.VITE_OPENWEATHERMAP_API_KEY}`,
    );
    const payload = await response.json();
    if (!response.ok) {
      throw new Error(payload.message || "Failed to fetch weather data");
    }
    return payload as WeatherData;
  } catch (error) {
    console.warn("Error fetching weather data:", error);
    throw error;
  }
};

export const useGetWeather = (
  params: WeatherParams,
  options?: UseQueryOptions<WeatherData, Error>,
): UseQueryResult<WeatherData, Error> => {
  const query = useQuery<WeatherData, Error>({
    queryKey: ["weather", params],
    queryFn: () =>
      "city" in params
        ? getWeatherByCity(params.city)
        : getWeatherByCoordinates(params.lat, params.lon),
    ...options,
    staleTime: 1000 * 60 * 5,
    retry: 1,
  });
  return query;
};
