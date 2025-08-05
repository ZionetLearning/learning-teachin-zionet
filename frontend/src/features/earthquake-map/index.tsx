import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import { useGetEarthquakes } from "./api";
import "leaflet/dist/leaflet.css";

export const EarthquakeMap = () => {
  const { data: quakes, isLoading, error } = useGetEarthquakes();

  if (isLoading) return <p>Loading map...</p>;
  if (error) return <p>Error fetching data</p>;

  console.log("Earthquakes data:", quakes);

  return (
    <MapContainer
      center={[20, 0]}
      zoom={2}
      style={{ height: "100vh", width: "100%" }}
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
