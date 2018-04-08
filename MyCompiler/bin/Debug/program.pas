program helloworld;
var
	char f[3];
	int mas[10];
	int x, y, a, b, c1, i;
end
begin
	write("Enter the array of char element: ");
	readln(f[1]);

	write("Enter the array of char element2: ");
	readln(f[2]);

	write("Enter x: ");
	readln(x);

	write("Enter y: ");
	readln(y);

	mas[x-6] = 2*(3-y); //some good comment for variant 2
	c1 = mas[x-6];
	if [x<y] and [y>5] then
		a = x+y+8
	else
		a = 5*x;
	if ([x != 6] or true) then
	begin
		b = x-c1;
		writeln("b = ", b);
	end;
	x = 0;
	while ([x < 25])
	begin
		x = x+5;
		y = (y+2)*4;
	end;
	writeln("a = ", a);
	writeln("x = ", x);
	writeln("y = ", y);
	writeln("Cycle FOR type Basic, step 3 :");

	x = 0;
	for mas[x] = 1 to 22 step 3
		mas[x] = mas[x]-1;
		write(mas[x], " ");
	next mas[x];
end.