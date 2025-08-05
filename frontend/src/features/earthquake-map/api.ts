import { useQuery } from "@tanstack/react-query";
import { Earthquake } from "./types";

const USGS_API =
  "https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&starttime=now-1day";

export const useGetEarthquakes = () => {
  return useQuery<Earthquake[], Error>({
    queryKey: ["earthquakes"],
    queryFn: async () => {
      const res = await fetch(USGS_API);
      const data = await res.json();
      return data.features as Earthquake[];
    },
  });
};
