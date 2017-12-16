#include <cstdlib>
#include <iostream>

int main() {

	system("sudo hdparm -I /dev/sda | grep Number");
	system("sudo hdparm -I /dev/sda | grep Firmware");

	// info about HDD memory
	system( "df | awk '{hddVolume += $2 / (1024*1024); hddUsedVolume += $3 / (1024*1024); hddAvailableVolume+=$4 / (1024*1024)} "
		"END {print \"\\tHDD volume: \" hddVolume \" Gb\\n\" "
		"\"\\tHDD used volume: \" hddUsedVolume \" Gb\\n\" "
		"\"\\tHDD available volume: \" hddAvailableVolume \" Gb\"}'");

	system("sudo hdparm -I /dev/sda | grep PIO");
	system("sudo hdparm -I /dev/sda | grep DMA");
	system("sudo hdparm -I /dev/sda | grep Supported");
	
	return 0;
}
