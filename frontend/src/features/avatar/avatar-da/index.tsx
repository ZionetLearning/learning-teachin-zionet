import { Canvas } from '@react-three/fiber';
import { Experience } from './Experience';

export const AvatarDa = () => {
	return (
		<div style={{ width: 'calc(100vw - 18rem)', height: 'calc(100vh - 4rem)' }}>
			<Canvas shadows camera={{ position: [0, 0, 8], fov: 42 }}>
				<color attach="background" args={['#000']} />
				<Experience />
			</Canvas>
		</div>
	);
};
