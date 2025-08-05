import axios, { AxiosInstance } from 'axios';

import { initAxios } from '@/services';
import { WeatherData } from '@/types';

export class ApiService {
	private axiosInstance: AxiosInstance;

	constructor() {
		this.axiosInstance = initAxios();
	}

	async getWeatherByCoordinates(
		lat: number,
		lon: number
	): Promise<WeatherData> {
		try {
			const response = await this.axiosInstance.get<WeatherData>(
				`https://api.openweathermap.org/data/2.5/weather?lat=${lat}&lon=${lon}&units=metric&appid=${import.meta.env.VITE_OPENWEATHERMAP_API_KEY}`
			);
			return response.data;
		} catch (error) {
			if (axios.isAxiosError(error) && error.response) {
				if (error.response.status === 404)
					throw new Error('Location not found');
			}
			console.error('Error fetching weather data:', error);
			throw error;
		}
	}

	async getWeatherByCity(city: string): Promise<WeatherData> {
		try {
			const response = await this.axiosInstance.get<WeatherData>(
				`https://api.openweathermap.org/data/2.5/weather?q=${city}&units=metric&appid=${import.meta.env.VITE_OPENWEATHERMAP_API_KEY}`
			);
			return response.data;
		} catch (error) {
			if (axios.isAxiosError(error) && error.response) {
				if (error.response.status === 404) throw new Error('City not found');
			}
			console.error('Error fetching weather data:', error);
			throw error;
		}
	}
}
