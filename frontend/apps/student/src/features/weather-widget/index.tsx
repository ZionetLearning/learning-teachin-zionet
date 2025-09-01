import { useState } from "react";

import { WeatherParams } from "@student/types";
import { useGetWeather } from "./api";

import { useStyles } from "./style";
import { useTranslation } from "react-i18next";

export const WeatherWidget = () => {
  const classes = useStyles();
  const { t } = useTranslation();
  const [selected, setSelected] = useState<string | null>(null);
  const [search, setSearch] = useState<string>("");
  const [locating, setLocating] = useState(false);
  const [coords, setCoords] = useState<{ lat: number; lon: number } | null>(
    null,
  );

  const presetCities = [
    t("pages.weather.cities.newYork"),
    t("pages.weather.cities.london"),
    t("pages.weather.cities.tokyo"),
    t("pages.weather.cities.paris"),
    t("pages.weather.cities.berlin"),
    t("pages.weather.cities.sydney"),
    t("pages.weather.cities.rioDeJaneiro"),
    t("pages.weather.cities.capeTown"),
    t("pages.weather.cities.mumbai"),
  ];

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
          <option value="">{t("pages.weather.selectCityOrLocation")}</option>
          <option value="location">{t("pages.weather.useMyLocation")}</option>
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
            placeholder={t("pages.weather.searchCity")}
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          />
          <button className={classes.button} onClick={handleSearch}>
            {t("pages.weather.search")}
          </button>
        </div>
      </div>

      {(locating ||
        (isLoading && !locating) ||
        error ||
        (!locating && !isLoading && !error && !data)) && (
        <div className={classes.messageContainer}>
          {locating && (
            <div className={classes.loading}>{t("pages.weather.locating")}</div>
          )}
          {!locating && isLoading && (
            <div className={classes.loading}>
              {t("pages.weather.loadingWeather")}
            </div>
          )}
          {error && (
            <div className={classes.error}>
              {error.message === "city not found"
                ? "City not found. Please try again."
                : `Error: ${error.message}`}
            </div>
          )}
          {!locating && !isLoading && !error && !data && (
            <div className={classes.loading}>
              {t("pages.weather.selectOrSearchCity")}
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
            {t("pages.weather.feelsLike")} {Math.round(data.main.feels_like)}°C
            <br />
            {t("pages.weather.min")} {Math.round(data.main.temp_min)}°C |{" "}
            {t("pages.weather.max")}
            {Math.round(data.main.temp_max)}°C
            <br />
            {t("pages.weather.humidity")} {data.main.humidity}% |{" "}
            {t("pages.weather.pressure")} {data.main.pressure} hPa
            <br />
            {t("pages.weather.wind")} {data.wind.speed} m/s
            {data.wind.deg ? ` at ${data.wind.deg}°` : ""}
            {data.wind.gust ? ` (gusts ${data.wind.gust} m/s)` : ""}
            <br />
            {t("pages.weather.visibility")}{" "}
            {(data.visibility / 1000).toFixed(1)} {t("pages.weather.km")}
            <br />
            {t("pages.weather.sunrise")} {formatTime(data.sys.sunrise)} |{" "}
            {t("pages.weather.sunset")} {formatTime(data.sys.sunset)}
          </div>
        </>
      )}
    </div>
  );
};
