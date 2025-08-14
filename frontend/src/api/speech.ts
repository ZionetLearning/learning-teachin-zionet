import { useMutation } from '@tanstack/react-query';
import axios from 'axios';

export type VisemeEvent = {
	visemeId: number;
	offsetMs: number;
};

export type SynthesizeResponse = {
	audioData: string;
	visemes: Array<VisemeEvent>;
	metadata: {
		audioLength: number;
		audioFormat: string;
		processingDuration: string;
	};
};

export type SynthesizerRequest = {
	text: string;
};

export const useSynthesizeSpeech = () => {
	return useMutation<SynthesizeResponse, Error, SynthesizerRequest>({
		mutationFn: async ({ text }: SynthesizerRequest) => {
			const response = await axios.post<SynthesizeResponse>(
				'https://teachin.westeurope.cloudapp.azure.com/api/dev/speech/synthesize',
				{ text }
			);
			return response.data;
		},
	});
};
