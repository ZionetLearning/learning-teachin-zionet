import axios, { AxiosInstance } from 'axios';

export const initAxios = (): AxiosInstance => {
	const newInstance = axios.create();
	return newInstance;
};
