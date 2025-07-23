import { Canvas } from "@react-three/fiber";
import { Leva } from "leva";

import { Scene } from "./Scene";

import { useStyles } from "./style";

export const AvatarDa = () => {
  const classes = useStyles();
  return (
    <>
      <Leva hidden />
      <Canvas
        className={classes.fullScreenCanvas}
        shadows
        camera={{ position: [0, 0, 8], fov: 42 }}
      >
        <Scene />
      </Canvas>
    </>
  );
};
