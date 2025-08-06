import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import { useGetEarthquakes } from "./api";
import "leaflet/dist/leaflet.css";
import { useState } from "react";
import { useStyles } from "./style";

import {
  Box,
  FormControl,
  MenuItem,
  Select,
  SelectChangeEvent,
  Typography,
} from "@mui/material";

export const EarthquakeMap = () => {
  const classes = useStyles();
  const [hoursAgo, setHoursAgo] = useState(24);
  const { data: quakes, isLoading, error } = useGetEarthquakes(hoursAgo);

  const handleChange = (event: SelectChangeEvent) => {
    setHoursAgo(Number(event.target.value));
  };

  return (
    <>
      <Box className={classes.dropdownWrapper}>
        <Typography className={classes.dropdownTitle}>
          Show earthquakes from:
        </Typography>
        <FormControl className={classes.formControl} size="small">
          <Select value={hoursAgo.toString()} onChange={handleChange}>
            <MenuItem value={24}>Last 24 hours</MenuItem>
            <MenuItem value={48}>Last 48 hours</MenuItem>
            <MenuItem value={72}>Last 72 hours</MenuItem>
            <MenuItem value={168}>Last 7 days</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {isLoading && <p>Loading map...</p>}
      {error && <p>Error fetching data</p>}

      <MapContainer
        center={[20, 0]}
        zoom={2}
        style={{ height: "calc(100vh - 60px)", width: "100%" }}
      >
        {/* defines where Leaflet should load the map tiles from, in this case from openstreetmap */}
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
    </>
  );
};
