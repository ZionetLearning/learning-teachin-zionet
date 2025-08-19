import { useQuery } from '@tanstack/react-query';
import { fetchCountries, Country, CountryQueryParams } from '../api';

export const useCountriesQuery = (params: CountryQueryParams) => {
  return useQuery<Country[], Error>({
    queryKey: ['countries', params.region ?? 'All'], // region drives server fetch
    queryFn: () => fetchCountries(params),
    staleTime: 10 * 60 * 1000, // 10 min
  });
}