type SvgUrl = string;

const modules = import.meta.glob('./*.svg', {
	query: '?url',
	import: 'default',
	eager: true,
});

export const lipsArray = Object.values(modules) as SvgUrl[];
