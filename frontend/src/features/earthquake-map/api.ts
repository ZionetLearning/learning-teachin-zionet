import { useQuery } from "@tanstack/react-query";
import { Earthquake } from "./types";

// USGS => United States Geological Survey, Earthquake Open API endpoint
const buildEarthquakeUrl = (hoursAgo: number) => {
  const end = new Date().toISOString();
  // hoursAgo * 60 * 60 * 1000 => calculating the hours in miliseconds
  const start = new Date(Date.now() - hoursAgo * 60 * 60 * 1000).toISOString();
  console.log(`Fetching earthquakes from ${start} to ${end}`);
  return `https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&starttime=${start}&endtime=${end}&minmagnitude=3`;
};

export const useGetEarthquakes = (hoursAgo: number) => {
  return useQuery<Earthquake[], Error>({
    queryKey: ["earthquakes", hoursAgo],
    queryFn: async () => {
      const res = await fetch(buildEarthquakeUrl(hoursAgo));
      const data = await res.json();
      return data.features as Earthquake[];
    },
  });
};
