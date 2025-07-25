interface SendIconProps {
  width: number;
  height: number;
  className?: string;
  fill?: string;
  stroke?: string;
  onClick?: () => void;
}

export const SendIcon = ({
  width,
  height,
  className,
  fill,
  stroke,
  onClick,
}: SendIconProps) => (
  <svg
    width={width}
    height={height}
    viewBox="0 0 24 24"
    xmlns="http://www.w3.org/2000/svg"
    id="send"
    className={className}
    fill={fill}
    stroke={stroke}
    onClick={onClick}
  >
    <path d="M21.66,12a2,2,0,0,1-1.14,1.81L5.87,20.75A2.08,2.08,0,0,1,5,21a2,2,0,0,1-1.82-2.82L5.46,13H11a1,1,0,0,0,0-2H5.46L3.18,5.87A2,2,0,0,1,5.86,3.25h0l14.65,6.94A2,2,0,0,1,21.66,12Z"></path>
  </svg>
);
