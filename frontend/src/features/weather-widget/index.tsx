import { useState } from "react";

import { useGetWeather } from "@/hooks";
import { WeatherParams } from "@/types";

import { useStyles } from "./style";

const presetCities = [
  "New York",
  "London",
  "Tokyo",
  "Paris",
  "Berlin",
  "Sydney",
  "Rio de Janeiro",
  "Cape Town",
  "Mumbai",
];

export const WeatherWidget = () => {
  const classes = useStyles();
  const [selected, setSelected] = useState<string | null>(null);
  const [search, setSearch] = useState<string>("");
  const [locating, setLocating] = useState(false);
  const [coords, setCoords] = useState<{ lat: number; lon: number } | null>(
    null,
  );

  const params: WeatherParams | null =
    selected === "location"
      ? coords
        ? { lat: coords.lat, lon: coords.lon }
        : null
      : selected
        ? { city: selected }
        : null;

  const { data, error, isLoading } = useGetWeather(params as WeatherParams, {
    queryKey: ["weather", params],
    enabled: !!params,
  });

  const handleSelectLocation = (value: string) => {
    setSelected(value || null);
    if (value === "location" && navigator.geolocation) {
      setLocating(true);
      navigator.geolocation.getCurrentPosition(
        ({ coords: { latitude, longitude } }) => {
          setCoords({ lat: latitude, lon: longitude });
          setLocating(false);
        },
        () => {
          setLocating(false);
          setSelected(null);
          console.warn("Location access denied or not supported");
        },
      );
    }
  };

  const handleSearch = () => {
    if (search.trim()) {
      setSelected(search);
      setSearch("");
    }
  };

  const formatTime = (unix: number) => {
    if (!data) return "";
    const local = new Date((unix + data.timezone) * 1000);
    return local.toLocaleTimeString([], {
      hour: "2-digit",
      minute: "2-digit",
      timeZone: "UTC",
    });
  };

  return (
    <div className={classes.container}>
      <div className={classes.inputGroup}>
        <select
          name="select-city"
          className={classes.select}
          value={selected || ""}
          onChange={(e) => handleSelectLocation(e.target.value)}
        >
          <option value="">-- Select City or Location --</option>
          <option value="location">Use My Location</option>
          {presetCities.map((city) => (
            <option key={city} value={city}>
              {city}
            </option>
          ))}
        </select>

        <div className={classes.searchGroup}>
          <input
            name="search-city"
            className={classes.input}
            placeholder="Search city..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          />
          <button className={classes.button} onClick={handleSearch}>
            Search
          </button>
        </div>
      </div>

      {(locating ||
        (isLoading && !locating) ||
        error ||
        (!locating && !isLoading && !error && !data)) && (
        <div className={classes.messageContainer}>
          {locating && <div className={classes.loading}>Locating...</div>}
          {!locating && isLoading && (
            <div className={classes.loading}>Loading weather...</div>
          )}
          {error && (
            <div className={classes.error}>
              {error.message === "City not found"
                ? "City not found. Please try again."
                : `Error: ${error.message}`}
            </div>
          )}
          {!locating && !isLoading && !error && !data && (
            <div className={classes.loading}>
              Select or search a city or use your location to see the weather.
            </div>
          )}
        </div>
      )}

      {data && (
        <>
          <h3 className={classes.heading}>{data.name}</h3>
          <div className={classes.weatherContainer}>
            <div className={classes.iconContainer}>
              <img
                className={classes.icon}
                src={`https://openweathermap.org/img/wn/${data.weather[0].icon}@2x.png`}
                alt={data.weather[0].description}
              />
              <p className={classes.temp}>{Math.round(data.main.temp)}°C</p>
            </div>
            <p className={classes.description}>{data.weather[0].description}</p>
          </div>
          <div className={classes.stats}>
            Feels like: {Math.round(data.main.feels_like)}°C
            <br />
            Min: {Math.round(data.main.temp_min)}°C | Max:
            {Math.round(data.main.temp_max)}°C
            <br />
            Humidity: {data.main.humidity}% | Pressure: {data.main.pressure} hPa
            <br />
            Wind: {data.wind.speed} m/s
            {data.wind.deg ? ` at ${data.wind.deg}°` : ""}
            {data.wind.gust ? ` (gusts ${data.wind.gust} m/s)` : ""}
            <br />
            Visibility: {(data.visibility / 1000).toFixed(1)} km
            <br />
            Sunrise: {formatTime(data.sys.sunrise)} | Sunset:
            {formatTime(data.sys.sunset)}
          </div>
        </>
      )}
    </div>
  );
};
