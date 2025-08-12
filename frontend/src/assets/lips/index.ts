type SvgUrl = string;

const modules = import.meta.glob("./*.svg", { eager: true, as: "url" });

export const lipsArray = Object.values(modules) as SvgUrl[];
