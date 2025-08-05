export type Earthquake = {
  id: string;
  properties: {
    mag: number;
    place: string;
    time: number;
  };
  geometry: {
    coordinates: [number, number]; // [lng, lat]
  };
};
