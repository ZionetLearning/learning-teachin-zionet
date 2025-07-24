import { Environment, useTexture } from "@react-three/drei";
import { useThree } from "@react-three/fiber";

import { Avatar } from "../components/Avatar";
import { backgroundJpg } from "../assets";

export const Scene = () => {
  const texture = useTexture(backgroundJpg);
  const viewport = useThree((state) => state.viewport);
  return (
    <>
      <Avatar position={[0, -3, 5]} scale={2} />
      <Environment preset="sunset" />
      <mesh>
        <planeGeometry args={[viewport.width, viewport.height]} />
        <meshBasicMaterial map={texture} />
      </mesh>
    </>
  );
};
