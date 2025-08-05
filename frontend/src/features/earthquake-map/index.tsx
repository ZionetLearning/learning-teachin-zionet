import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import { useQuery } from "@tanstack/react-query";
import "leaflet/dist/leaflet.css";

const USGS_API =
  "https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&starttime=2025-08-01&endtime=2025-08-05&minmagnitude=4";

type Earthquake = {
  id: string;
  properties: {
    mag: number;
    place: string;
    time: number;
  };
  geometry: {
    coordinates: [number, number]; // [longitude, latitude]
  };
};

const fetchEarthquakes = async () => {
  const res = await fetch(USGS_API);
  const data = await res.json();
  return data.features as Earthquake[];
};

export const EarthquakeMap = () => {
  const {
    data: quakes,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["earthquakes"],
    queryFn: fetchEarthquakes,
  });

  if (isLoading) return <p>Loading map...</p>;
  if (error) return <p>Error fetching data</p>;

  return (
    <MapContainer
      center={[20, 0]}
      zoom={2}
      style={{ height: "600px", width: "100%" }}
    >
      <TileLayer
        attribution='&copy; <a href="https://openstreetmap.org">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
      {quakes?.map((eq) => {
        const [lng, lat] = eq.geometry.coordinates;
        return (
          <Marker key={eq.id} position={[lat, lng]}>
            <Popup>
              <strong>{eq.properties.place}</strong>
              <br />
              Magnitude: {eq.properties.mag}
              <br />
              {new Date(eq.properties.time).toLocaleString()}
            </Popup>
          </Marker>
        );
      })}
    </MapContainer>
  );
};
