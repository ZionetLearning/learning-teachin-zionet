import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";

import { useGetEarthquakes } from "./api";
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
import { useTranslation } from "react-i18next";

const customMarkerIcon = new L.Icon({
  iconUrl: "/map-marker.png",
  iconSize: [38, 38],
});

export const EarthquakeMap = () => {
  const classes = useStyles();
  const { t } = useTranslation();

  const [hoursAgo, setHoursAgo] = useState(24);
  const { data: quakes, isLoading, error } = useGetEarthquakes(hoursAgo);

  const handleChange = (event: SelectChangeEvent) => {
    setHoursAgo(Number(event.target.value));
  };

  return (
    <>
      <Box
        className={classes.dropdownWrapper}
        data-testid="eq-dropdown-wrapper"
      >
        <Typography className={classes.dropdownTitle}>
          {t("pages.earthquakeMap.dropdownLabel")}
        </Typography>
        <FormControl className={classes.formControl} size="small">
          <Select
            data-testid="eq-timeframe"
            value={hoursAgo.toString()}
            onChange={handleChange}
          >
            <MenuItem value={24}>
              {t("pages.earthquakeMap.last24Hours")}
            </MenuItem>
            <MenuItem value={48}>
              {t("pages.earthquakeMap.last48Hours")}
            </MenuItem>
            <MenuItem value={72}>
              {t("pages.earthquakeMap.last72Hours")}
            </MenuItem>
            <MenuItem value={168}>
              {t("pages.earthquakeMap.last7Days")}
            </MenuItem>
          </Select>
        </FormControl>
      </Box>

      {isLoading && <p>{t("pages.earthquakeMap.loadingMap")}</p>}
      {error && <p>{t("pages.earthquakeMap.error")}</p>}

      <div data-testid="eq-map">
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
              <Marker key={eq.id} position={[lat, lng]} icon={customMarkerIcon}>
                <Popup>
                  <strong>{eq.properties.place}</strong>
                  <br />
                  {t("pages.earthquakeMap.magnitude")} {eq.properties.mag}
                  <br />
                  {new Date(eq.properties.time).toLocaleString()}
                </Popup>
              </Marker>
            );
          })}
        </MapContainer>
      </div>
    </>
  );
};
